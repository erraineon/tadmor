using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Tadmor.Utils
{
    public class AsyncConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
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