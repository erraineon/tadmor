using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.Commands.Extensions;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Preference.Services
{
    public class ContextualPreferencesProvider : IContextualPreferencesProvider
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;

        public ContextualPreferencesProvider(IGuildPreferencesRepository guildPreferencesRepository)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
        }

        public async Task<Preferences> GetContextualPreferencesAsync(IGuildChannel channel, IGuildUser user)
        {
            var guildPreferences = await _guildPreferencesRepository.GetGuildPreferencesAsyncOrNull(channel.GuildId);
            var orderedPreferences = FindPreferencesForContext(
                channel,
                user,
                guildPreferences);
            var reducedPreferences = Reduce(orderedPreferences);
            return reducedPreferences;
        }

        private static IEnumerable<Preferences> FindPreferencesForContext(
            IGuildChannel channel,
            IGuildUser user,
            GuildPreferences? guildPreferences)
        {
            if (guildPreferences == null) yield break;
            yield return guildPreferences;

            if (TryGetChannelPreferences(guildPreferences, channel.Id, out var channelPreferences))
                yield return channelPreferences;

            foreach (var roleId in user.RoleIds)
                if (TryGetRolePreferences(guildPreferences, roleId, out var rolePreferences))
                    yield return rolePreferences;

            if (TryGetUserPreferences(guildPreferences, user.Id, out var guildUserPreferences))
                yield return guildUserPreferences;

            if (channelPreferences != null)
            {
                foreach (var roleId in user.RoleIds)
                    if (TryGetRolePreferences(channelPreferences, roleId, out var rolePreferences))
                        yield return rolePreferences;

                if (TryGetUserPreferences(channelPreferences, user.Id, out var channelUserPreferences))
                    yield return channelUserPreferences;
            }
        }

        private static bool TryGetRolePreferences(
            IGroupPreferencesContainer groupPreferencesContainer,
            ulong roleId,
            [NotNullWhen(true)] out RolePreferences? rolePreferences)
        {
            return groupPreferencesContainer.RolePreferences.TryGetValue(roleId, out rolePreferences);
        }

        private static bool TryGetUserPreferences(
            IGroupPreferencesContainer groupPreferencesContainer,
            ulong userId,
            [NotNullWhen(true)] out UserPreferences? userPreferences)
        {
            return groupPreferencesContainer.UserPreferences.TryGetValue(userId, out userPreferences);
        }

        private static bool TryGetChannelPreferences(
            GuildPreferences guildPreferences,
            ulong channelId,
            [NotNullWhen(true)] out ChannelPreferences? channelPreferences)
        {
            return guildPreferences.ChannelPreferences.TryGetValue(channelId, out channelPreferences);
        }

        // TODO: adopt convention-based approach rather than copying properties one by one
        private static Preferences Reduce(IEnumerable<Preferences> orderedPreferences)
        {
            return orderedPreferences.Aggregate(
                new Preferences(),
                (carry, value) =>
                {
                    if (!string.IsNullOrWhiteSpace(value.CommandPrefix))
                        carry.CommandPrefix = value.CommandPrefix;
                    foreach (var commandPermission in value.CommandPermissions)
                        carry.CommandPermissions.AddOrUpdate(commandPermission);
                    carry.Rules.AddRange(value.Rules);
                    return carry;
                });
        }
    }
}