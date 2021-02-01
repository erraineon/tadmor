using System.Threading.Tasks;
using Humanizer;
using Tadmor.Abstractions.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Commands.Formatters
{
    public class RuleFormatter : IStringFormatter<RuleBase>
    {
        public Task<string> ToStringAsync(RuleBase value)
        {
            string formattedValue;
            if (value is Reminder reminder)
            {
                formattedValue =
                    $"in {reminder.Delay.Humanize()} (runs at {reminder.NextRunDate}), remind {reminder.Username}: {reminder.ReminderText}";
            }
            else
            {
                var formattedValuePrefix = value switch
                {
                    RegexRule regexRule => $"on `{regexRule.Trigger}`",
                    CronRule cronRule =>
                        $"with cron schedule `{cronRule.CronSchedule}` (next run: {cronRule.NextRunDate})",
                    RecurringRule recurringRule =>
                        $"every {recurringRule.Interval.Humanize()} (next run: {recurringRule.NextRunDate})",
                    OneTimeRule oneTimeRule =>
                        $"in {oneTimeRule.Delay.Humanize()} (runs at: {oneTimeRule.NextRunDate})",
                    _ => value.ToString()
                };
                formattedValue = $"{formattedValuePrefix} do: `{value.Reaction}`";
            }

            return Task.FromResult(formattedValue);
        }
    }
}