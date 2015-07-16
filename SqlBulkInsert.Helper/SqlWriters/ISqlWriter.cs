using System.Collections.Generic;
using System.Data;

namespace SqlBulkInsert.Helper.SqlWriters
{
    public interface ISqlWriter<T>
    {
        void Write(IDbTransaction transaction, List<T> items);
    }
}