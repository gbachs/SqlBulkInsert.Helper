using System;
using System.Data;

namespace SqlBulkInsert.Helper.Extensions
{
    public static class IDbConnectionExtensions
    {
        public static IDbCommand CreateTextCommand(this IDbConnection thisObj, string sql)
        {
            return CreateCommand(thisObj, sql, CommandType.Text);
        }

        internal static IDbCommand CreateCommand(this IDbConnection thisObj, string text, CommandType cmdType)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = thisObj.CreateCommand();
                cmd.Connection = thisObj;
                cmd.CommandText = text;
                cmd.CommandType = cmdType;
                return cmd;
            }
            catch (Exception)
            {
                cmd.TryDispose();
                throw;
            }
        }

        public static int ExecuteNonQuery(this IDbConnection thisObj, string sql)
        {
            using (var cmd = thisObj.CreateTextCommand(sql))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public static void WithinTransaction(this IDbConnection thisObj, Action<IDbTransaction> action)
        {
            using (var transaction = thisObj.BeginTransaction())
            {
                action(transaction);
                transaction.Commit();
            }
        }
    }
}