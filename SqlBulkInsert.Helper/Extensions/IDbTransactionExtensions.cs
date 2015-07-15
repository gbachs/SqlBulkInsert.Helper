using System;
using System.Data;

namespace SqlBulkInsert.Helper.Extensions
{
    public static class IDbTransactionExtensions
    {
        public static IDbCommand CreateTextCommand(this IDbTransaction thisObj, string sql)
        {
            return CreateCommand(thisObj, sql, CommandType.Text);
        }

        private static IDbCommand CreateCommand(this IDbTransaction thisObj, string text, CommandType cmdType)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = thisObj.Connection.CreateCommand();
                cmd.CommandText = text;
                cmd.CommandType = cmdType;
                cmd.Transaction = thisObj;
                return cmd;
            }
            catch (Exception)
            {
                cmd.TryDispose();
                throw;
            }
        }
    }
}