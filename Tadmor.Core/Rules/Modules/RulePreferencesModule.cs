using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Formatting.Interfaces;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;
using Tadmor.Core.Preference.Modules;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Modules
{
    public class RulePreferencesModule : PreferencesModuleBase
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;
        private readonly IStringFormatter<PreferencesScope> _preferenceScopeFormatter;
        private readonly IStringFormatter<RuleBase> _ruleFormatter;

        public RulePreferencesModule(
            IGuildPreferencesRepository guildPreferencesRepository,
            IStringFormatter<RuleBase> ruleFormatter,
            IStringFormatter<PreferencesScope> preferenceScopeFormatter) : base(guildPreferencesRepository)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
            _ruleFormatter = ruleFormatter;
            _preferenceScopeFormatter = preferenceScopeFormatter;
        }

        [Command("rules")]
        public async Task<RuntimeResult> ListRules()
        {
            var guildPreferences = await _guildPreferencesRepository.GetGuildPreferencesAsyncOrNull(Context.Guild.Id) ??
                new GuildPreferences();
            var flattenedPreferences = FlattenPreferences(guildPreferences);

            var ruleStrings = await Task.WhenAll(
                flattenedPreferences
                    .SelectMany(t => t.Item2.Rules, (t, rule) => (scope: t.Item1, rule))
                    .Select(
                        async (t, i) =>
                        {
                            var scope = await _preferenceScopeFormatter.ToStringAsync(t.scope);
                            var rule = await _ruleFormatter.ToStringAsync(t.rule);
                            return $"{i}{scope}: {rule}";
                        }));
            return ruleStrings.Any()
                ? CommandResult.FromSuccess(ruleStrings)
                : CommandResult.FromError("there are no rules on this guild");
        }

        [Command("rules rm")]
        public async Task<RuntimeResult> RemoveRules(params int[] ruleIndexes)
        {
            var guildPreferences = await _guildPreferencesRepository.GetGuildPreferencesAsyncOrNull(Context.Guild.Id) ??
                new GuildPreferences();
            var removalTasks = FlattenPreferences(guildPreferences)
                .SelectMany(t => t.Item2.Rules, (t, rule) => (scope: t.Item1, rule))
                .Where((_, i) => ruleIndexes.Contains(i))
                .Select(
                    t => _guildPreferencesRepository.UpdatePreferencesAsync(
                        preferences => preferences.Rules.Remove(t.rule),
                        Context.Guild.Id,
                        t.scope))
                .ToList();
            await Task.WhenAll(removalTasks);
            return CommandResult.FromSuccess($"removed {removalTasks.Count} rules");
        }

        private static IEnumerable<(PreferencesScope, Preferences)> FlattenPreferences(
            GuildPreferences guildPreferences)
        {
            var flattenedPreferences = new[]
                {
                    (new PreferencesScope(default, default, default), (Preferences) guildPreferences)
                }
                .Concat(
                    guildPreferences.RolePreferences
                        .Select(p => (new PreferencesScope(default, default, p.Key), (Preferences) p.Value)))
                .Concat(
                    guildPreferences.UserPreferences
                        .Select(p => (new PreferencesScope(default, p.Key, default), (Preferences) p.Value)))
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
                                            p => (new PreferencesScope(cp.Key, default, p.Key), (Preferences) p.Value)))
                                .Concat(
                                    cp.Value.UserPreferences
                                        .Select(
                                            p => (new PreferencesScope(cp.Key, p.Key, default), (Preferences) p.Value)))
                        ));
            return flattenedPreferences;
        }
    }
}