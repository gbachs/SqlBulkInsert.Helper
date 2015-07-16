using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SqlBulkInsert.Helper.SqlWriters
{
    public class PostgresSqlWriter<T> : ISqlWriter<T>
    {
        private readonly SqlMetadata<T> _metadata;
        private readonly SqlBulkCopyOptions _bulkCopyOptions;

        public PostgresSqlWriter()
        {
            _metadata = Cache.GetOrCreate(typeof (T).ToString(), () => new SqlMetadata<T>());
            _bulkCopyOptions = SqlBulkCopyOptions.Default;
        }


        public void Write(IDbTransaction transaction, List<T> items)
        {
            if (items == null || !(items.Any()))
                return;

            lock (GetLocker())
            {

            }
        }

        private static object GetLocker()
        {
            return Cache.GetOrCreate(string.Concat(typeof(T).ToString(), "_lock"), () => new object());
        }
    }
}