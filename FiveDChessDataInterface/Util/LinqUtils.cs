using System;
using System.Collections.Generic;
using System.Linq;

namespace FiveDChessDataInterface.Util {
    internal static class LinqUtils {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source) {
                if (seenKeys.Add(keySelector(element))) {
                    yield return element;
                }
            }
        }

        public static TSource MaxElement<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selectorFunc) {
            if (!source.Any())
                throw new InvalidOperationException("Sequence was empty!");

            TSource result = source.First();
            int maxValue = selectorFunc.Invoke(result);

            foreach (var item in source.Skip(1)) {
                var currVal = selectorFunc.Invoke(item);
                if (currVal > maxValue) {
                    result = item;
                    maxValue = currVal;
                }
            }

            return result;
        }

        public static TSource MinElement<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selectorFunc) {
            if (!source.Any())
                throw new InvalidOperationException("Sequence was empty!");

            TSource result = source.First();
            int minValue = selectorFunc.Invoke(result);

            foreach (var item in source.Skip(1)) {
                var currVal = selectorFunc.Invoke(item);
                if (currVal < minValue) {
                    result = item;
                    minValue = currVal;
                }
            }

            return result;
        }
    }
}
