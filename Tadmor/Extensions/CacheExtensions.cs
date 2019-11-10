using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Tadmor.Extensions
{
    public static class CacheExtensions
    {
        private static readonly ConcurrentDictionary<object, SemaphoreSlim> Semaphores =
            new ConcurrentDictionary<object, SemaphoreSlim>();
        public static async Task<TItem> GetOrCreateAsyncLock<TItem>(
            this IMemoryCache cache,
            object key,
            Func<ICacheEntry, Task<TItem>> factory)
        {
            var semaphore = Semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1));
            await semaphore.WaitAsync();
            try
            {
                return await cache.GetOrCreateAsync(key, factory);
            }
            finally
            {
                semaphore.Release();
                Semaphores.Remove(key, out _);
            }
        }

        public static IEnumerable<object> GetKeys(this IMemoryCache cache)
        {
            var property = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            if (property?.GetValue(cache) is ICollection collection)
            {
                PropertyInfo? methodInfo = null;
                foreach (var item in collection)
                {
                    methodInfo ??= item?.GetType().GetProperty("Key");
                    var val = methodInfo?.GetValue(item);
                    if (val != null) yield return val;
                }
            }
        }
    }
}