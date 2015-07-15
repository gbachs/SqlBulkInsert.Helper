using System;

namespace SqlBulkInsert.Helper.Extensions
{
    public static class ObjectExtensions
    {
        public static bool TryDispose(this object toDispose)
        {
            var disposable = toDispose as IDisposable;
            if (disposable == null)
                return false;

            disposable.Dispose();
            return true;
        }
    }
}