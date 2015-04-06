using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AadAppRoleManager.Web.Utilities
{
    public static class FunctionalExtensions
    {
        public static TReturn Maybe<T, TReturn>(this T obj, Func<T, TReturn> func)
            where T : class
        {
            return obj != null
                ? func(obj) 
                : default(TReturn);
        }

        public static TReturn Maybe<T, TReturn>(this T? obj, Func<T, TReturn> func)
            where T : struct
        {
            return obj.HasValue
                ? func(obj.Value)
                : default(TReturn);
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue @default)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : @default;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue @default)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : @default;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> kvps)
        {
            return kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static Dictionary<TKey, TValue> ZipToDictionary<TKey, TValue>(this IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            var dict = new Dictionary<TKey, TValue>();
            using (var enumerator1 = keys.GetEnumerator())
            using (var enumerator2 = values.GetEnumerator())
            {
                while (enumerator1.MoveNext() && enumerator2.MoveNext())
                    dict.Add(enumerator1.Current, enumerator2.Current);
            }
            return dict;
        }

        public static IEnumerable<TResult> Zip3<T1, T2, T3, TResult>(
            this IEnumerable<T1> first, IEnumerable<T2> second, IEnumerable<T3> third, 
            Func<T1, T2, T3, TResult> projection)
        {
            using (var enumerator1 = first.GetEnumerator())
            using (var enumerator2 = second.GetEnumerator())
            using (var enumerator3 = third.GetEnumerator())
            {
                while (enumerator1.MoveNext() && enumerator2.MoveNext() && enumerator3.MoveNext())
                {
                    yield return projection(enumerator1.Current, enumerator2.Current, enumerator3.Current);
                }
            }
        }

        public static IEnumerable<IReadOnlyList<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
        {
            if (batchSize < 1)
                throw new ArgumentOutOfRangeException("batchSize", batchSize, "batchSize must be greater than 0");

            using (var enumerator = items.GetEnumerator())
            {
                var canMoveNext = true;
                while (canMoveNext)
                {
                    var batch = new List<T>();
                    for (int i = 0; i < batchSize && (canMoveNext = enumerator.MoveNext()); i++)
                        batch.Add(enumerator.Current);

                    if (batch.Count > 0)
                        yield return batch;
                }
            }
        }

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return Task.WhenAll(tasks);
        } 
    }
}