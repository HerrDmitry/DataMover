﻿using System;
using System.Collections.Generic;

namespace Importer
{
    public static class GenericExtensions
    {
        public static Func<T> GetNextFunc<T>(this IList<T> list)
        {
            var listIndex = -1;
            return () =>
            {
                listIndex++;
                return listIndex < list.Count ? list[listIndex] : default(T);
            };
        }

        public static Func<T> GetNextFunc<T>(this IEnumerable<T> list)
        {
            var enumerator = list?.GetEnumerator();
            return () => enumerator?.MoveNext() == true ? enumerator.Current : default(T);
        }
    }
}