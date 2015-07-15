using System;
using System.Collections.Generic;

namespace SqlBulkInsert.Helper
{
    public static class Cache
    {
        private static readonly Dictionary<string, object> CachedItems = new Dictionary<string, object>();

        public static void Add(string key, object obj)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must be set.");
            }
            if (obj == null)
            {
                obj = new NullObject();
            }


            lock (CachedItems)
            {
                if (CachedItems.ContainsKey(key))
                    CachedItems.Remove(key);

                CachedItems.Add(key, obj);
            }
        }

        public static bool TryGet<T>(string key, out T value) where T : class
        {
            lock (CachedItems)
            {
                object returnValue;
                var result = CachedItems.TryGetValue(key, out returnValue);
                value = (T)returnValue;

                return result;
            }
        }

        public static T GetOrCreate<T>(string key, Func<T> createValue) where T : class
        {
            T returnValue;

            if (TryGet(key, out returnValue))
                return returnValue;

            returnValue = createValue();

            Add(key, returnValue);

            return returnValue;
        }

        private class NullObject { }
    }
}