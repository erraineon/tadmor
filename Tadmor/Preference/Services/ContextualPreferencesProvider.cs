using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Tadmor.Commands.Models;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Services
{
    public class ContextualPreferencesProvider : IContextualPreferencesProvider
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;

        public ContextualPreferencesProvider(IGuildPreferencesRepository guildPreferencesRepository)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
        }

        public async Task<Preferences> GetContextualPreferences(IGuildChannel channel, IGuildUser user)
        {
            var guildPreferences = await _guildPreferencesRepository.GetGuildPreferences(channel.GuildId);
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

            var roleIds = (user.RoleIds ?? Enumerable.Empty<ulong>()).ToList();

            if (TryGetChannelPreferences(guildPreferences, channel.Id, out var channelPreferences))
                yield return channelPreferences;

            foreach (var roleId in roleIds)
                if (TryGetRolePreferences(guildPreferences, roleId, out var rolePreferences))
                    yield return rolePreferences;

            if (TryGetUserPreferences(guildPreferences, user.Id, out var guildUserPreferences))
                yield return guildUserPreferences;

            if (channelPreferences != null)
            {
                foreach (var roleId in roleIds)
                    if (TryGetRolePreferences(channelPreferences, roleId, out var rolePreferences))
                        yield return rolePreferences;

                if (TryGetUserPreferences(channelPreferences, user.Id, out var channelUserPreferences)) yield return channelUserPreferences;
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

        private static Preferences Reduce(IEnumerable<Preferences> orderedPreferences)
        {
            return orderedPreferences.Aggregate(
                new Preferences(),
                (carry, value) =>
                {
                    if (!string.IsNullOrWhiteSpace(value.CommandPrefix))
                        carry.CommandPrefix = value.CommandPrefix;
                    var relevantPermissions = value.CommandPermissions
                        .Where(valueCommand => valueCommand.CommandPermissionType != CommandPermissionType.None)
                        .ToList();
                    carry.CommandPermissions
                        .RemoveAll(carryCommand => relevantPermissions
                            .Any(valueCommand => carryCommand.CommandName == valueCommand.CommandName));
                    carry.CommandPermissions.AddRange(relevantPermissions);
                    return carry;
                });
        }
    }
}