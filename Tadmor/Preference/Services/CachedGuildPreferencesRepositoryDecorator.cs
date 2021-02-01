using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Services
{
    public class CachedGuildPreferencesRepositoryDecorator : IGuildPreferencesRepository
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;
        private readonly IMemoryCache _memoryCache;

        public CachedGuildPreferencesRepositoryDecorator(IGuildPreferencesRepository guildPreferencesRepository, IMemoryCache memoryCache)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
            _memoryCache = memoryCache;
        }

        public async Task<GuildPreferences?> GetGuildPreferencesAsyncOrNull(ulong guildId)
        {
            var cacheKey = GetCacheKey(guildId);
            var preferences = await _memoryCache.GetOrCreateAsync(cacheKey, cacheEntry =>
            {
                cacheEntry.SlidingExpiration = TimeSpan.FromDays(1);
                return _guildPreferencesRepository.GetGuildPreferencesAsyncOrNull(guildId);
            });
            return preferences;
        }

        private static string GetCacheKey(ulong guildId) =>
            $"guild-{guildId}-preferences";

        public async Task UpdatePreferencesAsync(Action<Preferences> updateAction,
            ulong guildId,
            PreferencesScope preferencesScope)
        {
            await _guildPreferencesRepository.UpdatePreferencesAsync(updateAction, guildId, preferencesScope);
            var cacheKey = GetCacheKey(guildId);
            _memoryCache.Remove(cacheKey);
        }
    }
}