using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NCrontab;
using Tadmor.Abstractions.Interfaces;
using Tadmor.Commands.Models;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Preference.Modules;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Modules
{
    public class RulePreferencesModule : PreferencesModuleBase
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;
        private readonly IStringFormatter<PreferencesScope> _preferenceScopeFormatter;
        private readonly IStringFormatter<RuleBase> _ruleFormatter;
        private readonly ITimeRulePreferencesWriter _timeRulePreferencesWriter;

        public RulePreferencesModule(
            IGuildPreferencesRepository guildPreferencesRepository,
            IStringFormatter<RuleBase> ruleFormatter,
            IStringFormatter<PreferencesScope> preferenceScopeFormatter,
            ITimeRulePreferencesWriter timeRulePreferencesWriter) : base(guildPreferencesRepository)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
            _ruleFormatter = ruleFormatter;
            _preferenceScopeFormatter = preferenceScopeFormatter;
            _timeRulePreferencesWriter = timeRulePreferencesWriter;
        }

        [Command("remind")]
        public async Task<RuntimeResult> Remind(TimeSpan delay, [Remainder] string reminder)
        {
            var rule = await AddTimeRuleAsync(
                () =>
                    new Reminder(Context.User.Username, Context.User.Mention, delay, reminder));
            return CommandResult.FromSuccess($"will remind at {rule.NextRunDate}");
        }

        [Command("in")]
        public async Task<RuntimeResult> AddOneTimeRule(TimeSpan delay, [Remainder] string reaction)
        {
            await AddTimeRuleAsync(() => new OneTimeRule(delay, reaction));
            return CommandResult.FromSuccess("added one-time rule");
        }

        [Command("every")]
        public async Task<RuntimeResult> AddRecurringRule(TimeSpan interval, [Remainder] string reaction)
        {
            await AddTimeRuleAsync(() => new RecurringRule(interval, reaction));
            return CommandResult.FromSuccess("added recurring rule");
        }

        [Command("cron")]
        public async Task<RuntimeResult> AddCronRule(CrontabSchedule cronSchedule, [Remainder] string reaction)
        {
            await AddTimeRuleAsync(() => new CronRule(cronSchedule.ToString(), reaction));
            return CommandResult.FromSuccess("added cron rule");
        }

        [Command("on")]
        public Task<RuntimeResult> AddRegexRule(string trigger, [Remainder] string reaction)
        {
            return AddRegexRule(trigger, reaction, new PreferencesScopeCommandModel());
        }

        [Command("on")]
        public async Task<RuntimeResult> AddRegexRule(
            string trigger,
            string reaction,
            PreferencesScopeCommandModel preferencesContext)
        {
            await WithPreferencesScope(
                preferencesContext,
                preferences => preferences.Rules.Add(new RegexRule(trigger, reaction)));
            return CommandResult.FromSuccess($"added regex rule for {preferencesContext}");
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

        private async Task<TimeRule> AddTimeRuleAsync(
            Func<TimeRule> timeRuleFactory)
        {
            var result = await _timeRulePreferencesWriter.UpdatePreferencesAsync(
                Context.Guild.Id,
                Context.Channel.Id,
                timeRuleFactory(),
                (preferences, rule) => preferences.Rules.Add(rule));
            return result;
        }
    }
}