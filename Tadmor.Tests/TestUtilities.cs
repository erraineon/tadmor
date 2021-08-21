using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using NSubstitute;

namespace Tadmor.Tests
{
    public static class TestUtilities
    {
        public static IMemoryCache CreateMemoryCache()
        {
            var memoryCache = Substitute.For<IMemoryCache>();
            var dictionary = new Dictionary<object, ICacheEntry>();
            memoryCache.TryGetValue(Arg.Any<object>(), out _)
                .Returns(x =>
                {
                    var found = dictionary.TryGetValue(x[0], out var entry);
                    if (found) x[1] = entry.Value;
                    return found;
                });
            memoryCache.CreateEntry(Arg.Any<object>())
                .Returns(x =>
                {
                    var key = x[0];
                    var entry = Substitute.For<ICacheEntry>();
                    entry.PostEvictionCallbacks.Returns(new List<PostEvictionCallbackRegistration>());
                    entry.ExpirationTokens
                        .When(y => y.Add(Arg.Any<IChangeToken>()))
                        .Do(y => y.Arg<IChangeToken>()
                            .RegisterChangeCallback(_ => memoryCache.Remove(key), default));
                    dictionary[key] = entry;
                    return entry;
                });
            memoryCache
                .When(x => x.Remove(Arg.Any<object>()))
                .Do(x =>
                {
                    var key = x[0];
                    if (dictionary.TryGetValue(key, out var entry))
                    {
                        dictionary.Remove(key);
                        foreach (var registration in entry.PostEvictionCallbacks)
                            registration.EvictionCallback(
                                key,
                                entry.Value,
                                EvictionReason.Removed,
                                registration.State);
                    }
                });
            return memoryCache;
        }
    }
}