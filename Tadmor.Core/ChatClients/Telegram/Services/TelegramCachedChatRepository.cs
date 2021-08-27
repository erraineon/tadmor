using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Tadmor.Core.Extensions;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramCachedChatRepository : ITelegramCachedChatRepository
    {
        private readonly IMemoryCache _memoryCache;

        public TelegramCachedChatRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<ITelegramGuild> GetOrCreateAsync(long guildId, Func<Task<ITelegramGuild>> factory)
        {
            var guild = await _memoryCache.GetOrCreateAsyncLock(
                GetTelegramChatKey(guildId),
                async e =>
                {
                    e.SlidingExpiration = TimeSpan.FromDays(1);
                    var guild = await factory();
                    var cachedChatIds = GetAllCachedChatIds();
                    cachedChatIds.Add(guildId);
                    return guild;
                });
            return guild;
        }

        public ICollection<long> GetAllChatIds()
        {
            var chatIds = GetAllCachedChatIds();
            return chatIds;
        }

        private static string GetTelegramChatKey(long guildId)
        {
            return $"telegram-chats-{guildId}";
        }

        private HashSet<long> GetAllCachedChatIds()
        {
            return _memoryCache.GetOrCreate("telegram-chats-list", _ => new HashSet<long>());
        }
    }
}