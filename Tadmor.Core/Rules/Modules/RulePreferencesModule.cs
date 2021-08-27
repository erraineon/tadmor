using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NCrontab;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Formatting.Interfaces;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;
using Tadmor.Core.Preference.Modules;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Modules
{
    [Summary("time and text based events")]
    public class RulePreferencesModule : PreferencesModuleBase
    {
        private readonly ITimeRulePreferencesWriter _timeRulePreferencesWriter;
        private readonly IStringFormatter<RuleBase> _ruleFormatter;

        public RulePreferencesModule(
            ITimeRulePreferencesWriter timeRulePreferencesWriter,
            IGuildPreferencesRepository guildPreferencesRepository,
            IStringFormatter<RuleBase> ruleFormatter,
            IStringFormatter<PreferencesScope> preferenceScopeFormatter) : base(guildPreferencesRepository, preferenceScopeFormatter)
        {
            _timeRulePreferencesWriter = timeRulePreferencesWriter;
            _ruleFormatter = ruleFormatter;
        }

        [Command("remind")]
        [Summary("sets a reminder in the specified amount of time")]
        public async Task<RuntimeResult> Remind(TimeSpan delay, [Remainder] string reminder)
        {
            if (delay <= TimeSpan.Zero) throw new ModuleException("can't set a reminder in the past");
            var rule = await AddTimeRuleAsync(
                () =>
                    new Reminder(Context.User.Username, Context.User.Mention, delay, reminder, Context.Client.CurrentUser.Id));
            return CommandResult.FromSuccess($"will remind at {rule.NextRunDate}");
        }

        [Command("remind")]
        [Summary("sets a reminder for a the specified time (eastern time)")]
        public Task<RuntimeResult> Remind(DateTime dateTime, [Remainder] string reminder)
        {
            return Remind(dateTime - DateTime.Now, reminder);
        }

        [Command("in")]
        [Summary("executes a command in the specified amount of time")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "trusted"), RequireOwner(Group = "trusted")]
        public async Task<RuntimeResult> AddOneTimeRule(TimeSpan delay, [Remainder] string command)
        {
            if (delay <= TimeSpan.Zero) throw new ModuleException("can't set a reminder in the past");
            var rule = await AddTimeRuleAsync(() => new OneTimeRule(delay, Context.User.Id, command));
            var formattedRule = await _ruleFormatter.ToStringAsync(rule);
            return CommandResult.FromSuccess($"added rule: {formattedRule}");
        }

        [Command("at")]
        [Summary("executes a command at the specified time (eastern time)")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "trusted"), RequireOwner(Group = "trusted")]
        public Task<RuntimeResult> AddOneTimeRule(DateTime dateTime, [Remainder] string command)
        {
            return AddOneTimeRule(dateTime - DateTime.Now, command);
        }

        [Command("every")]
        [Summary("executes a command at the specified interval")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "trusted"), RequireOwner(Group = "trusted")]
        public async Task<RuntimeResult> AddRecurringRule(TimeSpan interval, [Remainder] string command)
        {
            var rule = await AddTimeRuleAsync(() => new RecurringRule(interval, Context.User.Id, command));
            var formattedRule = await _ruleFormatter.ToStringAsync(rule);
            return CommandResult.FromSuccess($"added rule: {formattedRule}");
        }

        [Command("cron")]
        [Summary("executes a command at the cron tab schedule")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "trusted"), RequireOwner(Group = "trusted")]
        public async Task<RuntimeResult> AddCronRule(CrontabSchedule cronSchedule, [Remainder] string command)
        {
            var rule = await AddTimeRuleAsync(() => new CronRule(cronSchedule.ToString(), Context.User.Id, command));
            var formattedRule = await _ruleFormatter.ToStringAsync(rule);
            return CommandResult.FromSuccess($"added rule: {formattedRule}");
        }

        [Command("on")]
        [HideInHelp]
        [RequireUserPermission(GuildPermission.Administrator, Group = "trusted"), RequireOwner(Group = "trusted")]
        public Task<RuntimeResult> AddRegexRule(string trigger, [Remainder] string command)
        {
            return AddRegexRule(trigger, command, new PreferencesScopeCommandModel());
        }

        [Command("on")]
        [Summary("executes a command when a message contains the specified regex trigger, optionally for a given context")]
        [Priority(1)]
        [RequireUserPermission(GuildPermission.Administrator, Group = "trusted"), RequireOwner(Group = "trusted")]
        public async Task<RuntimeResult> AddRegexRule(
            string trigger,
            string command,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            PreferencesScopeCommandModel? preferencesContext = default)
        {
            preferencesContext ??= new PreferencesScopeCommandModel();
            var rule = new RegexRule(trigger, command);
            await WithPreferencesScope(
                preferencesContext,
                preferences => preferences.Rules.Add(rule));
            var formattedRule = await _ruleFormatter.ToStringAsync(rule);
            return CommandResult.FromSuccess($"added rule: {formattedRule} for {preferencesContext}");
        }

        [Command("rules")]
        [Summary("lists the rules for this guild")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "trusted"), RequireOwner(Group = "trusted")]
        public async Task<RuntimeResult> ListRules()
        {
            var ruleStrings = await FormatPreferencesAsync(p => p.Rules, _ruleFormatter);
            return ruleStrings.Any()
                ? CommandResult.FromSuccess(ruleStrings)
                : CommandResult.FromError("there are no rules set for this guild");
        }

        [Command("rules rm")]
        [Summary("removes the rules at the specified indexes")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "trusted"), RequireOwner(Group = "trusted")]
        public async Task<RuntimeResult> RemoveRules(params int[] ruleIndexes)
        {
            var removedRules = await RemovePreferencesAsync(p => p.Rules, ruleIndexes);
            return CommandResult.FromSuccess($"removed {removedRules} rules");
        }

        private async Task<TimeRule> AddTimeRuleAsync(
            Func<TimeRule> timeRuleFactory)
        {
            var result = await _timeRulePreferencesWriter.UpdatePreferencesAsync(Context.Guild.Id, Context.Channel.Id,
                timeRuleFactory(),
                (preferences, rule) => preferences.Rules.Add(rule));
            return result;
        }
    }
}