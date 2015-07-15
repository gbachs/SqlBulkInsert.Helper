﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SqlBulkInsert.Helper.Extensions;

namespace SqlBulkInsert.Helper
{
    public class SqlWriter<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, object> TableLocks = new Dictionary<Type, object>();
        private readonly SqlMetadata<T> _metadata;

        public SqlWriter()
        {
            _metadata = Cache.GetOrCreate(typeof(T).ToString(), () => new SqlMetadata<T>());
            BulkCopyOptions = SqlBulkCopyOptions.Default;
        }

        public string TableName
        {
            get { return _metadata.TableName; }
            set { _metadata.TableName = value; }
        }

        public SqlBulkCopyOptions BulkCopyOptions { get; set; }

        public void Write(IDbTransaction transaction, List<T> items)
        {
            if (items == null || !(items.Any()))
                return;

            lock (GetLocker())
            {
                BulkSaveItems(transaction, items);

                if (_metadata.GeneratedIdProperty != null)
                    LoadGeneratedIdsAndAssignTreeParentIds(transaction, items);
            }
        }

        private static object GetLocker()
        {
            return TableLocks.GetOrCreate(typeof(T), () => new object());
        }

        private void BulkSaveItems(IDbTransaction transaction, IEnumerable<T> list)
        {
            var sqlBulkCopy = CreateSqlBulkCopy((SqlTransaction)transaction);
            using (var dataReader = new SqlMetadataReader<T>(_metadata.AllProperties, list))
            {
                sqlBulkCopy.WriteToServer(dataReader);
            }
        }

        private SqlBulkCopy CreateSqlBulkCopy(SqlTransaction transaction)
        {
            var sqlBulkCopy = new SqlBulkCopy(transaction.Connection, BulkCopyOptions, transaction)
            {
                DestinationTableName = _metadata.TableName,
                BulkCopyTimeout = 500000
            };
            AddBulkCopyMappings(sqlBulkCopy);
            return sqlBulkCopy;
        }

        private void AddBulkCopyMappings(SqlBulkCopy sqlBulkCopy)
        {
            _metadata.Properties.ForEach(x => sqlBulkCopy.ColumnMappings.Add(x.PropertyName, x.ColumnName));

            if (_metadata.ContainerIdProperties != null && _metadata.ContainerIdProperties.Count > 0)
            {
                _metadata.ContainerIdProperties.ForEach(
                    x => sqlBulkCopy.ColumnMappings.Add(x.PropertyName, x.ColumnName));
            }
        }

        private void LoadGeneratedIdsAndAssignTreeParentIds(IDbTransaction transaction, List<T> list)
        {
            //if we are only dealing with flat lists of data then there is no need to load a datatable from a sqldataadapter becuase that is slow
            if (_metadata.TreeParentIdProperty == null)
            {
                using (var command = CreateSelectIdsCmd(transaction, list))
                {
                    var newIds = command.Select(x => x.GetValue(0)).ToList();
                    for (var idx = 0; idx < list.Count; idx++)
                    {
                        _metadata.GeneratedIdProperty.SetValue(list[idx], newIds[idx]);
                    }
                }
            }
            else
            {
                using (var selectCmd = CreateSelectIdsCmd(transaction, list))
                using (var adapter = new SqlDataAdapter((SqlCommand)selectCmd))
                using (var entityTable = new DataTable())
                {
                    adapter.Fill(entityTable);
                    var newIds = entityTable.Rows.Cast<DataRow>().Select(x => x[0]).ToList();

                    for (var idx = 0; idx < list.Count; idx++)
                        _metadata.GeneratedIdProperty.SetValue(list[idx], newIds[idx]);

                    if (_metadata.TreeParentIdProperty != null)
                        AssignTreeParentIds(transaction, list, adapter, entityTable);
                }
            }
        }

        private IDbCommand CreateSelectIdsCmd(IDbTransaction transaction, IList<T> list)
        {
            var sql = new StringBuilder();

            if (_metadata.TreeParentIdProperty == null)
            {
                sql.Append(string.Format("SELECT TOP {0} {1} FROM {2} WHERE ", list.Count,
                    _metadata.GeneratedIdProperty.ColumnName, _metadata.TableName));
            }
            else
            {
                sql.Append(string.Format("SELECT TOP {0} {1},{2} FROM {3} WHERE ", list.Count,
                    _metadata.GeneratedIdProperty.ColumnName, _metadata.TreeParentIdProperty.ColumnName,
                    _metadata.TableName));
            }

            var andOperator = "";

            for (var i = 0; i < _metadata.ContainerIdProperties.Count; i++)
            {
                if (i == 1)
                    andOperator = " AND ";

                var containerIdProperty = _metadata.ContainerIdProperties[i];

                if (containerIdProperty.IsSingleValue || containerIdProperty.GetValue(list[0]) == null)
                {
                    var value = containerIdProperty.GetValue(list[0]);
                    sql.Append(value == null
                        ? string.Format(" {0} IS NULL", containerIdProperty.ColumnName)
                        : string.Format(" {0} = {1}", containerIdProperty.ColumnName, value));
                }
                else
                {
                    var containerIds = list.Select(containerIdProperty.GetValue).Distinct();

                    var containerIdsString = string.Join(",", containerIds.Select(x => x.ToString()).ToArray());

                    sql.Append(string.Format("{0}{1} IN ({2})", andOperator, containerIdProperty.ColumnName, containerIdsString));
                }
            }

            sql.Append(string.Format(" ORDER BY {0}", _metadata.GeneratedIdProperty.ColumnName));

            return transaction.CreateTextCommand(sql.ToString());
        }

        private IDbCommand CreateUpdateTreeParentIdsCmd(IDbTransaction transaction)
        {
            var sql = string.Format("UPDATE {0} SET {1} = @{1} WHERE {2}=@{2}",
                _metadata.TableName, _metadata.TreeParentIdProperty.ColumnName, _metadata.GeneratedIdProperty.ColumnName);
            var updateCmd = (SqlCommand)transaction.CreateTextCommand(sql);
            updateCmd.UpdatedRowSource = UpdateRowSource.None;

            var parentIdParam = updateCmd.Parameters.Add("@" + _metadata.TreeParentIdProperty.ColumnName,
                SqlDbType.BigInt);
            parentIdParam.SourceVersion = DataRowVersion.Current;
            parentIdParam.SourceColumn = _metadata.TreeParentIdProperty.ColumnName;

            var idParam = updateCmd.Parameters.Add("@" + _metadata.GeneratedIdProperty.ColumnName, SqlDbType.BigInt);
            idParam.SourceVersion = DataRowVersion.Current;
            idParam.SourceColumn = _metadata.GeneratedIdProperty.ColumnName;

            return updateCmd;
        }

        private void AssignTreeParentIds(IDbTransaction transaction, List<T> list, SqlDataAdapter adapter, DataTable entityTable)
        {
            using (var updateCmd = CreateUpdateTreeParentIdsCmd(transaction))
            {
                adapter.UpdateCommand = (SqlCommand)updateCmd;
                const int allUpdatesInOneBatch = 0;
                adapter.UpdateBatchSize = allUpdatesInOneBatch;

                for (var idx = 0; idx < list.Count; idx++)
                {
                    var rawParentIdx = _metadata.TreeParentIdProperty.GetValue<long?>(list[idx]);
                    if (rawParentIdx == null) continue;

                    var parentIdx = (int)rawParentIdx.Value;
                    var parentId = _metadata.GeneratedIdProperty.GetValue<long>(list[parentIdx]);
                    entityTable.Rows[idx][1] = parentId;

                    //TODO read the parent id after save
                    _metadata.TreeParentIdProperty.SetValue(list[idx], parentId);
                }
                adapter.Update(entityTable);
            }
        }
    }
}