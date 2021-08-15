using System;
using System.Threading.Tasks;
using LinqToTwitter;
using Microsoft.Extensions.Caching.Memory;
using Tadmor.Core.Extensions;
using Tadmor.Twitter.Interfaces;

namespace Tadmor.Twitter.Services
{
    public class CachedTwitterContextFactoryDecorator : ITwitterContextFactory
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ITwitterContextFactory _twitterContextFactory;

        public CachedTwitterContextFactoryDecorator(IMemoryCache memoryCache, ITwitterContextFactory twitterContextFactory)
        {
            _memoryCache = memoryCache;
            _twitterContextFactory = twitterContextFactory;
        }

        public Task<TwitterContext> CreateAsync()
        {
            var key = $"twittercontext";
            return _memoryCache.GetOrCreateAsyncLock(key, e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                return _twitterContextFactory.CreateAsync();
            });
        }
    }
}