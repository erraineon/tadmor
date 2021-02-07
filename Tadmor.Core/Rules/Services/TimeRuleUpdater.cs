using System.Threading;
using System.Threading.Tasks;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;
using Tadmor.Core.Rules.Notifications;

namespace Tadmor.Core.Rules.Services
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