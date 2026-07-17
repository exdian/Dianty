using System;
using System.Collections.Generic;

namespace Dianty.Utils;

public static class EnumerableExtension
{
    extension<T>(IEnumerable<T> source) where T : allows ref struct
    {
        public int FirstIndex(Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                    return i;
                i++;
            }
            return -1;
        }

        public int LastIndex(Func<T, bool> predicate)
        {
            int i = 0;
            int result = -1;
            foreach (var item in source)
            {
                if (predicate(item))
                    result = i;
                i++;
            }
            return result;
        }
    }
}
