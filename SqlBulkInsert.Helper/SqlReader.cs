using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using SqlBulkInsert.Helper.Extensions;

namespace SqlBulkInsert.Helper
{
    public class SqlReader<T>
        where T : new()
    {
        private readonly SqlMetadata<T> _metadata;

        public SqlReader()
        {
            _metadata = new SqlMetadata<T>();
        }

        public List<T> Load(IDbTransaction transaction, IList<long> containerIds)
        {
            var fieldsString = string.Join(",", _metadata.AllProperties.Select(x => x.ColumnName).ToArray());
            var loadedContainerIds = 0;
            var returnValues = new List<T>();
            while (loadedContainerIds < containerIds.Count())
            {
                var containerIdsToLoad = containerIds.Skip(loadedContainerIds).Take(1000).ToList();
                var containerIdsString = string.Join(",", containerIdsToLoad.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
                var sql = string.Format("SELECT {0} FROM {1} WHERE {2} IN ({3})",
                                                 fieldsString, _metadata.TableName, _metadata.ContainerIdProperties[0].ColumnName,
                                                 containerIdsString);
                returnValues = returnValues.Concat(transaction.Select(sql, CreateItem).ToList()).ToList();
                loadedContainerIds += containerIdsToLoad.Count();
            }

            return returnValues;
        }

        private T CreateItem(IDataRecord row)
        {
            var item = new T();
            var colIdx = 0;
            _metadata.AllProperties.ForEach(p => p.SetValue(item, row[colIdx++]));
            return item;
        }
    }
}