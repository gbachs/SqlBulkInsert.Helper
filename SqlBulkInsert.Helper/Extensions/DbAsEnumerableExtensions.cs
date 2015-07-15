using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlBulkInsert.Helper.Extensions
{
    public static class DbAsEnumerableExtensions
    {
        /// <summary>
        /// foreach (row in reader.AsEnumerable())
        /// OR
        /// reader.Select(row => new Thing { Name = row.GetString(0) }).FirstOrDefault();
        /// </summary>
        public static IEnumerable<IDataRecord> AsEnumerable(this IDataReader reader)
        {
            while (reader.Read())
                yield return reader;
        }

        /// <summary>
        /// reader.Select(row => new Thing { name = row.GetString(0) }).ToList();
        /// </summary>
        public static IEnumerable<T> Select<T>(this IDataReader reader, Func<IDataRecord, T> selector)
        {
            return reader.AsEnumerable().Select(selector);
        }

        /// <summary>
        /// cmd.Select().Select(row => CreateObject(row)).ToList();
        /// Like the below, but there are a few cases where having direct access to the reader is important.
        /// </summary>
        public static IEnumerable<IDataRecord> Select(this IDbCommand command)
        {
            return command.Select(r => r);
        }

        /// <summary>
        /// cmd.Select(row => CreateObject(row)).ToList();
        /// </summary>
        public static IEnumerable<T> Select<T>(this IDbCommand command, Func<IDataRecord, T> selector)
        {
            using (var reader = command.ExecuteReader())
            {
                foreach (var record in reader.Select(selector))
                    yield return record;
            }
        }

        public static IEnumerable<IDataRecord> Select(this IDbTransaction trans, string sql, Action<IDbCommand> setupCommand)
        {
            using (var cmd = trans.CreateTextCommand(sql))
            {
                setupCommand(cmd);
                foreach (var record in cmd.Select())
                    yield return record;
            }
        }

        public static IEnumerable<T> Select<T>(this IDbTransaction trans, string sql, Action<IDbCommand> setupCommand, Func<IDataRecord, T> selector)
        {
            return trans.Select(sql, setupCommand).Select(selector);
        }


        public static IEnumerable<T> Select<T>(this IDbTransaction trans, string sql, Func<IDataRecord, T> selector)
        {
            return trans.Select(sql, cmd => { }, selector);
        }
    }
}