using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Tadmor.Utils
{
    // TODO: use the notnull type constraint once ReSharper supports it
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    public class AsyncConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    {
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> factory)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!TryGetValue(key, out var value))
                {
                    this[key] = value = await factory(key);
                }

                return value;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}