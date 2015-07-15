using System;
using System.Collections.Generic;

namespace SqlBulkInsert.Helper.Extensions
{
   public static class IDictionaryExtensions
	{
	    public static TItem GetOrCreate<TKey, TItem>(this IDictionary<TKey, TItem> thisObj, TKey key, Func<TItem> createValue)
	    {
	        TItem returnValue;

            if(thisObj.TryGetValue(key, out returnValue))
                return returnValue;

	        returnValue = createValue();
            thisObj.Add(key, returnValue);

            return returnValue;
	    }
    }
}
