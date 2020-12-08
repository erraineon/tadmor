﻿using System;
using System.Threading.Tasks;
using Tadmor.Data.Models;
using Tadmor.Data.Services;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Models;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Services
{
    public class GuildPreferencesRepository : IGuildPreferencesRepository
    {
        private readonly TadmorDbContext _dbContext;
        private readonly INotificationPublisher _publisher;

        public GuildPreferencesRepository(
            TadmorDbContext dbContext,
            INotificationPublisher publisher)
        {
            _dbContext = dbContext;
            _publisher = publisher;
        }

        public async Task UpdatePreferences(Action<Preferences> updateAction,
            ulong guildId,
            PreferencesScope preferencesScope)
        {
            var guildPreferencesEntity = await GetOrCreateGuildPreferencesEntity(guildId);
            var preferences = GetMostSpecificPreferences(guildPreferencesEntity.Preferences, preferencesScope);
            updateAction(preferences);
            await _dbContext.SaveChangesAsync();
            await _publisher.PublishAsync(new GuildPreferencesUpdatedNotification(guildId, preferences));
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

        private static ChannelPreferences GetOrCreateChannelPreferences(GuildPreferences guildPreferences, ulong channelId)
        {
            return guildPreferences.ChannelPreferences.TryGetValue(channelId, out var channelPreferences)
                ? channelPreferences
                : guildPreferences.ChannelPreferences[channelId] = new ChannelPreferences();
        }

        private static UserPreferences GetOrCreateUserPreferences(IGroupPreferencesContainer groupPreferencesContainer, ulong userId)
        {
            return groupPreferencesContainer.UserPreferences.TryGetValue(userId, out var userPreferences)
                ? userPreferences
                : groupPreferencesContainer.UserPreferences[userId] = new UserPreferences();
        }

        private static RolePreferences GetOrCreateRolePreferences(IGroupPreferencesContainer groupPreferencesContainer, ulong userId)
        {
            return groupPreferencesContainer.RolePreferences.TryGetValue(userId, out var rolePreferences)
                ? rolePreferences
                : groupPreferencesContainer.RolePreferences[userId] = new RolePreferences();
        }

        public async Task<GuildPreferences?> GetGuildPreferences(ulong guildId)
        {
            var guildPreferencesEntity = await _dbContext.GuildPreferences.FindAsync(guildId);
            return guildPreferencesEntity?.Preferences;
        }
    }
}