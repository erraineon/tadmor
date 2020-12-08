using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Models;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Services
{
    public class 
    public class CachedContextualPreferencesProviderDecorator : IContextualPreferencesProvider, INotificationHandler<GuildMemberUpdatedNotification>, INotificationHandler<GuildPreferencesUpdatedNotification>
    {
        private readonly IContextualPreferencesProvider _contextualPreferencesProvider;
        private readonly IMemoryCache _memoryCache;

        public CachedContextualPreferencesProviderDecorator(
            IContextualPreferencesProvider contextualPreferencesProvider,
            IMemoryCache memoryCache)
        {
            _contextualPreferencesProvider = contextualPreferencesProvider;
            _memoryCache = memoryCache;
        }

        public async Task<Preferences> GetContextualPreferences(IGuildChannel channel, IGuildUser user)
        {
            var cacheKey = GetCacheKey(channel, user);
            var preferences = await _memoryCache.GetOrCreateAsync(cacheKey, cacheEntry =>
            {
                var guildEvictionToken = _memoryCache.GetOrCreate(GetGuildExpirationTokenKey(channel.GuildId), CreateEvictionToken);
                var userEvictionToken = _memoryCache.GetOrCreate(GetGuildUserExpirationTokenKey(user), CreateEvictionToken);
                cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);
                cacheEntry.ExpirationTokens.Add(guildEvictionToken);
                cacheEntry.ExpirationTokens.Add(userEvictionToken);
                return _contextualPreferencesProvider.GetContextualPreferences(channel, user);
            });

            return preferences;
        }

        private CancellationChangeToken CreateEvictionToken(ICacheEntry cacheEntry)
        {
            var cts = new CancellationTokenSource();
            cacheEntry.RegisterPostEvictionCallback((_, _, _, _) => cts.Cancel());
            cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);
            return new CancellationChangeToken(cts.Token);
        }

        private static string GetGuildExpirationTokenKey(ulong guildId)
        {
            return $"guild-{guildId}-contextual-preferences-eviction-tokens";
        }

        private static string GetGuildUserExpirationTokenKey(IGuildUser user)
        {
            return $"guild-{user.GuildId}-user-{user.Id}-contextual-preferences-eviction-tokens";
        }

        private static string GetCacheKey(IGuildChannel channel, IGuildUser user) =>
            $"guild-{channel.GuildId}-channel-{channel.Id}-user-{user.Id}-contextual-preferences";
        
        public Task HandleAsync(GuildMemberUpdatedNotification notification, CancellationToken cancellationToken)
        {
            var (_, oldUser, newUser) = notification;
            var (oldRoleIds, newRoleIds) = (oldUser.RoleIds, newUser.RoleIds);
            var rolesHaveChanged = oldRoleIds.Count != newRoleIds.Count || !new HashSet<ulong>(oldRoleIds).SetEquals(newRoleIds);
            if (rolesHaveChanged)
            {
                var expirationTokenKey = GetGuildUserExpirationTokenKey(newUser);
                _memoryCache.Remove(expirationTokenKey);
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(GuildPreferencesUpdatedNotification notification, CancellationToken cancellationToken)
        {
            var expirationTokenKey = GetGuildExpirationTokenKey(notification.GuildId);
            _memoryCache.Remove(expirationTokenKey);
            return Task.CompletedTask;
        }
    }
}