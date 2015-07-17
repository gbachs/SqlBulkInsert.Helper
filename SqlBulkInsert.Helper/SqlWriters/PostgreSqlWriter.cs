﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Npgsql;
using SqlBulkInsert.Helper.Extensions;

namespace SqlBulkInsert.Helper.SqlWriters
{
    public class PostgreSqlWriter<T> : ISqlWriter<T>
    {
        private readonly SqlMetadata<T> _metadata;

        public PostgreSqlWriter()
        {
            _metadata = Cache.GetOrCreate(typeof(T).ToString(), () => new SqlMetadata<T>());
        }

        public void Write(IDbTransaction transaction, List<T> list)
        {
            if (list == null || !(list.Any()))
                return;

            if (!(transaction is NpgsqlTransaction))
                throw new ApplicationException("This transaction type is not support with NpgSql");

            lock (GetLocker())
            {
                BulkInsertData((NpgsqlTransaction)transaction, list);

                if (_metadata.GeneratedIdProperty != null)
                    LoadGeneratedIds(transaction, list);
            }
        }

        private void LoadGeneratedIds(IDbTransaction transaction, List<T> list)
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

        private IDbCommand CreateSelectIdsCmd(IDbTransaction transaction, IList<T> list)
        {
            var sql = new StringBuilder();

            sql.Append(string.Format("SELECT \"{0}\" FROM \"{1}\" ",
                _metadata.GeneratedIdProperty.ColumnName, _metadata.TableName));

            var andOperator = " WHERE ";

            for (var i = 0; i < _metadata.ContainerIdProperties.Count; i++)
            {
                if (i == 1)
                    andOperator = " AND ";

                var containerIdProperty = _metadata.ContainerIdProperties[i];

                if (containerIdProperty.IsSingleValue || containerIdProperty.GetValue(list[0]) == null)
                {
                    var value = containerIdProperty.GetValue(list[0]);
                    sql.Append(value == null
                        ? string.Format(" \"{0}\" IS NULL", containerIdProperty.ColumnName)
                        : string.Format(" \"{0}\" = {1}", containerIdProperty.ColumnName, value));
                }
                else
                {
                    var containerIds = list.Select(containerIdProperty.GetValue).Distinct();

                    var containerIdsString = string.Join(",", containerIds.Select(x => x.ToString()).ToArray());

                    sql.Append(string.Format("{0} \"{1}\" IN ({2})", andOperator, containerIdProperty.ColumnName,
                        containerIdsString));
                }
            }

            sql.Append(string.Format(" ORDER BY \"{0}\"", _metadata.GeneratedIdProperty.ColumnName));
            sql.Append(string.Format(" LIMIT {0}", list.Count));
            return transaction.CreateTextCommand(sql.ToString());
        }

        private void BulkInsertData(NpgsqlTransaction transaction, IEnumerable<T> items)
        {
            using (var writer = transaction.Connection.BeginBinaryImport(CreateBinaryImportCommand()))
            using (var dataReader = new SqlMetadataReader<T>(_metadata.AllProperties, items))
            {
                while (dataReader.Read())
                {
                    writer.StartRow();
                    for (var i = 0; i < dataReader.FieldCount; i++)
                    {
                        writer.Write(dataReader[i]);
                    }
                }
            }
        }

        private string CreateBinaryImportCommand()
        {
            var commandString = new StringBuilder();

            commandString.Append(string.Format("COPY \"{0}\" (", _metadata.TableName));

            for (var i = 0; i < _metadata.AllProperties.Count; i++)
            {
                var property = _metadata.AllProperties[i];
                commandString.Append(string.Format("\"{0}\"", property.ColumnName));

                if (i != (_metadata.AllProperties.Count - 1))
                {
                    commandString.Append(",");
                }
            }

            commandString.Append(") FROM STDIN BINARY");

            return commandString.ToString();
        }

        private static object GetLocker()
        {
            return Cache.GetOrCreate(string.Concat(typeof(T).ToString(), "_lock"), () => new object());
        }
    }
}