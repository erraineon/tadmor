using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Formatting.Interfaces;
using Tadmor.Core.Preference.Extensions;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Preference.Modules
{
    public abstract class PreferencesModuleBase : ModuleBase<ICommandContext>
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;
        private readonly IStringFormatter<PreferencesScope> _preferenceScopeFormatter;

        protected PreferencesModuleBase(IGuildPreferencesRepository guildPreferencesRepository, IStringFormatter<PreferencesScope> preferenceScopeFormatter)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
            _preferenceScopeFormatter = preferenceScopeFormatter;
        }

        protected virtual async Task WithPreferencesScope(
            PreferencesScopeCommandModel preferencesContext,
            Action<Preferences> updateAction)
        {
            var updatePreferencesScope = new PreferencesScope(
                preferencesContext.Channel?.Id,
                preferencesContext.User?.Id,
                preferencesContext.Role?.Id);
            await _guildPreferencesRepository.UpdatePreferencesAsync(
                updateAction,
                Context.Guild.Id,
                updatePreferencesScope);
        }

        protected async Task<IList<(PreferencesScope, Preferences)>> GetAllScopesAndPreferencesAsync(ulong guildId)
        {
            var guildPreferences = await _guildPreferencesRepository.GetGuildPreferencesAsyncOrNull(guildId);
            if (guildPreferences == null) return ArraySegment<(PreferencesScope, Preferences)>.Empty;

            var preferencesList = new[]
                {
                    (new PreferencesScope(default, default, default), (Preferences) guildPreferences)
                }
                .Concat(
                    guildPreferences.RolePreferences
                        .Select(p => (new PreferencesScope(default, default, p.Key), (Preferences)p.Value)))
                .Concat(
                    guildPreferences.UserPreferences
                        .Select(p => (new PreferencesScope(default, p.Key, default), (Preferences)p.Value)))
                .Concat(
                    guildPreferences.ChannelPreferences
                        .SelectMany(
                            cp => new[]
                                {
                                    (new PreferencesScope(cp.Key, default, default), (Preferences) cp.Value)
                                }
                                .Concat(
                                    cp.Value.RolePreferences
                                        .Select(
                                            p => (new PreferencesScope(cp.Key, default, p.Key), (Preferences)p.Value)))
                                .Concat(
                                    cp.Value.UserPreferences
                                        .Select(
                                            p => (new PreferencesScope(cp.Key, p.Key, default), (Preferences)p.Value)))
                        ))
                .ToList();
            return preferencesList;
        }

        protected async Task<IList<string>> FormatPreferencesAsync<T>(Func<Preferences, IEnumerable<T>> selector, IStringFormatter<T> formatter)
        {
            var flattenedPreferences = await GetAllScopesAndPreferencesAsync(Context.Guild.Id);
            var strings = await Task.WhenAll(
                flattenedPreferences
                    .SelectMany(t => selector(t.Item2), (t, value) => (scope: t.Item1, value))
                    .Select(
                        async (t, i) =>
                        {
                            var scope = await _preferenceScopeFormatter.ToStringAsync(t.scope);
                            var formattedValue = await formatter.ToStringAsync(t.value);
                            return $"{i}: {formattedValue} {scope}";
                        }));
            return strings;
        }

        protected async Task<int> RemovePreferencesAsync<T>(Func<Preferences, IList<T>> selector, int[] indexesToRemove)
        {
            var flattenedPreferences = await GetAllScopesAndPreferencesAsync(Context.Guild.Id);
            var removalTasks = flattenedPreferences
                .SelectMany(t => selector(t.Item2), (t, value) => (scope: t.Item1, value))
                .Where((_, i) => indexesToRemove.Contains(i))
                .Select(
                    async t =>
                    {
                        var (scope, value) = t;
                        await _guildPreferencesRepository.UpdatePreferencesAsync(
                            preferences => selector(preferences).Remove(value),
                            Context.Guild.Id,
                            scope);
                    }).ToList();
            await Task.WhenAll(removalTasks);
            return removalTasks.Count;
        }
    }
}