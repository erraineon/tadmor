using System;
using System.Threading.Tasks;
using Tadmor.Core.Data.Interfaces;
using Tadmor.Core.Data.Models;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Preference.Services
{
    public class GuildPreferencesRepository : IGuildPreferencesRepository
    {
        private readonly ITadmorDbContext _dbContext;
        private readonly INotificationPublisher _publisher;

        public GuildPreferencesRepository(
            ITadmorDbContext dbContext,
            INotificationPublisher publisher)
        {
            _dbContext = dbContext;
            _publisher = publisher;
        }

        public async Task UpdatePreferencesAsync(
            Action<Preferences> updateAction,
            ulong guildId,
            PreferencesScope preferencesScope)
        {
            var guildPreferencesEntity = await GetOrCreateGuildPreferencesEntity(guildId);
            var preferences = GetMostSpecificPreferences(guildPreferencesEntity.Preferences, preferencesScope);
            updateAction(preferences);
            await _dbContext.SaveChangesAsync();
            await _publisher.PublishAsync(new GuildPreferencesUpdatedNotification(guildId, preferences));
        }

        public async Task<GuildPreferences?> GetGuildPreferencesAsyncOrNull(ulong guildId)
        {
            var guildPreferencesEntity = await _dbContext.GuildPreferences.FindAsync(guildId);
            return guildPreferencesEntity?.Preferences;
        }

        private async Task<GuildPreferencesEntity> GetOrCreateGuildPreferencesEntity(ulong guildId)
        {
            var guildPreferencesEntity = await _dbContext.GuildPreferences.FindAsync(guildId) ??
                await CreateGuildPreferencesEntity(guildId);
            return guildPreferencesEntity;
        }

        private async Task<GuildPreferencesEntity> CreateGuildPreferencesEntity(ulong guildId)
        {
            var guildPreferencesEntity = new GuildPreferencesEntity
            {
                GuildId = guildId,
                Preferences = new GuildPreferences()
            };
            await _dbContext.GuildPreferences.AddAsync(guildPreferencesEntity);
            return guildPreferencesEntity;
        }

        private static Preferences GetMostSpecificPreferences(
            GuildPreferences guildPreferences,
            PreferencesScope preferencesScope)
        {
            GroupPreferencesContainer locationScopedPreferences = preferencesScope.ChannelId is { } channelId
                ? GetOrCreateChannelPreferences(guildPreferences, channelId)
                : guildPreferences;

            Preferences result = preferencesScope switch
            {
                {UserId: { } userId} => GetOrCreateUserPreferences(locationScopedPreferences, userId),
                {RoleId: { } roleId} => GetOrCreateRolePreferences(locationScopedPreferences, roleId),
                _ => locationScopedPreferences
            };

            return result;
        }

        private static ChannelPreferences GetOrCreateChannelPreferences(
            GuildPreferences guildPreferences,
            ulong channelId)
        {
            return guildPreferences.ChannelPreferences.TryGetValue(channelId, out var channelPreferences)
                ? channelPreferences
                : guildPreferences.ChannelPreferences[channelId] = new ChannelPreferences();
        }

        private static UserPreferences GetOrCreateUserPreferences(
            IGroupPreferencesContainer groupPreferencesContainer,
            ulong userId)
        {
            return groupPreferencesContainer.UserPreferences.TryGetValue(userId, out var userPreferences)
                ? userPreferences
                : groupPreferencesContainer.UserPreferences[userId] = new UserPreferences();
        }

        private static RolePreferences GetOrCreateRolePreferences(
            IGroupPreferencesContainer groupPreferencesContainer,
            ulong userId)
        {
            return groupPreferencesContainer.RolePreferences.TryGetValue(userId, out var rolePreferences)
                ? rolePreferences
                : groupPreferencesContainer.RolePreferences[userId] = new RolePreferences();
        }
    }
}