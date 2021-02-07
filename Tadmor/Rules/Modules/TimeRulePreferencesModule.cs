using System;
using System.Threading.Tasks;
using Discord.Commands;
using NCrontab;
using Tadmor.Commands.Models;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Preference.Modules;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Modules
{
    public class TimeRulePreferencesModule : PreferencesModuleBase
    {
        private readonly ITimeRulePreferencesWriter _timeRulePreferencesWriter;

        public TimeRulePreferencesModule(
            ITimeRulePreferencesWriter timeRulePreferencesWriter,
            IGuildPreferencesRepository guildPreferencesRepository) : base(guildPreferencesRepository)
        {
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