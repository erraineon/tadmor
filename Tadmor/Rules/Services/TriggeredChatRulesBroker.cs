using System.Threading;
using System.Threading.Tasks;
using Tadmor.Commands.Models;
using Tadmor.Notifications.Interfaces;
using Tadmor.Preference.Interfaces;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Services
{
    public class TriggeredChatRulesBroker : INotificationHandler<MessageValidatedNotification>
    {
        private readonly IContextualPreferencesProvider _contextualPreferencesProvider;
        private readonly IRuleExecutor _ruleExecutor;

        public TriggeredChatRulesBroker(
            IContextualPreferencesProvider contextualPreferencesProvider,
            IRuleExecutor ruleExecutor)
        {
            _contextualPreferencesProvider = contextualPreferencesProvider;
            _ruleExecutor = ruleExecutor;
        }

        public async Task HandleAsync(MessageValidatedNotification notification, CancellationToken cancellationToken)
        {
            var preferences = await _contextualPreferencesProvider.GetContextualPreferences(
                notification.GuildChannel,
                notification.GuildUser);
            foreach (var rule in preferences.Rules)
            {
                var ruleTriggerContext = rule switch
                {
                    RegexRule regexRule => new RegexRuleTriggerContext(regexRule, notification),
                    _ => default
                };
                if (ruleTriggerContext?.ShouldExecute == true) await _ruleExecutor.ExecuteRuleAsync(ruleTriggerContext, cancellationToken);
            }
        }
    }
}