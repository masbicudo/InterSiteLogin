using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Masb.Yai
{
    internal static class InternalEnumerableExtensions
    {
        public static T[] ToArrayOrSelf<T>([NotNull] this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            var result = enumerable as T[];
            result = result ?? enumerable.ToArray();
            return result;
        }
    }
}