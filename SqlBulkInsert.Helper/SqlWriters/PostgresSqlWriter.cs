using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Npgsql;

namespace SqlBulkInsert.Helper.SqlWriters
{
    public class PostgresSqlWriter<T> : ISqlWriter<T>
    {
        private readonly SqlMetadata<T> _metadata;

        public PostgresSqlWriter()
        {
            _metadata = Cache.GetOrCreate(typeof(T).ToString(), () => new SqlMetadata<T>());
        }

        public void Write(IDbTransaction transaction, List<T> items)
        {
            if (items == null || !(items.Any()))
                return;

            if (!(transaction is NpgsqlTransaction))
                throw new ApplicationException("This transaction type is not support with NpgSql");

            lock (GetLocker())
            {
                BulkInsertData((NpgsqlTransaction)transaction, items);
            }
        }

        private void BulkInsertData(NpgsqlTransaction transaction, IEnumerable<T> items)
        {
            using (var writer = transaction.Connection.BeginBinaryImport(CreateCommand()))
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

        private string CreateCommand()
        {
            var commandString = new StringBuilder();

            commandString.Append(string.Format("COPY {0} (", _metadata.TableName));

            for (var i = 0; i < _metadata.AllProperties.Count; i++)
            {
                var property = _metadata.AllProperties[i];
                commandString.Append(property.ColumnName);

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