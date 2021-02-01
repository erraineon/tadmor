using System.Threading;
using System.Threading.Tasks;
using Tadmor.Notifications.Interfaces;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;
using Tadmor.Rules.Notifications;

namespace Tadmor.Rules.Services
{
    public class TimeRuleUpdater : INotificationHandler<RuleExecutedNotification>
    {
        private readonly ITimeRulePreferencesWriter _timeRulePreferencesWriter;

        public TimeRuleUpdater(
            ITimeRulePreferencesWriter timeRulePreferencesWriter)
        {
            _timeRulePreferencesWriter = timeRulePreferencesWriter;
        }

        public async Task HandleAsync(RuleExecutedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Rule is TimeRule timeRule)
                await _timeRulePreferencesWriter.UpdatePreferencesAsync(
                    notification.GuildId,
                    notification.ChannelId,
                    timeRule,
                    (preferences, updatedRule) =>
                    {
                        if (preferences.Rules.IndexOf(timeRule) is >= 0 and var oldRuleIndex)
                        {
                            preferences.Rules.RemoveAt(oldRuleIndex);
                            if (timeRule is not OneTimeRule)
                                preferences.Rules.Insert(oldRuleIndex, updatedRule);
                        }
                    });
        }
    }
}