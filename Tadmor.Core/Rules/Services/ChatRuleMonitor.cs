using System.Threading;
using System.Threading.Tasks;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Services
{
    public class ChatRuleMonitor : INotificationHandler<MessageValidatedNotification>
    {
        private readonly IContextualPreferencesProvider _contextualPreferencesProvider;
        private readonly IRuleExecutor _ruleExecutor;

        public ChatRuleMonitor(
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
                if (ruleTriggerContext?.ShouldExecute == true)
                    await _ruleExecutor.ExecuteRuleAsync(ruleTriggerContext, cancellationToken);
            }
        }
    }
}