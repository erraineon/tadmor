using System.Collections.Generic;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Tadmor.ChatClients.Telegram.Interfaces;

namespace Tadmor.ChatClients.Telegram.Services
{
    public class GuildUserCache : IGuildUserCache
    {
        private readonly IMemoryCache _cache;

        public GuildUserCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public IReadOnlyCollection<IGuildUser> GetGuildUsers(ulong guildId)
        {
            var guildUsers = _cache.GetOrCreate($"guild-{guildId}-users", e => new List<IGuildUser>());
            return guildUsers.AsReadOnly();
        }
    }
}