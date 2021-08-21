using System.Threading;
using System.Threading.Tasks;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.TextGeneration.Services
{
    public class GenerateWhenRepliedToBehaviour : INotificationHandler<MessageValidatedNotification>
    {
        private readonly IRuleExecutor _ruleExecutor;

        public GenerateWhenRepliedToBehaviour(IRuleExecutor ruleExecutor)
        {
            _ruleExecutor = ruleExecutor;
        }

        public async Task HandleAsync(MessageValidatedNotification notification, CancellationToken cancellationToken)
        {
            var myId = notification.ChatClient.CurrentUser.Id;
            if (notification.UserMessage.ReferencedMessage?.Author?.Id == myId)
            {
                var ruleContext = new ArbitraryExecutionTriggerContext("gen", notification);
                await _ruleExecutor.ExecuteRuleAsync(ruleContext, cancellationToken);
            }
        }
    }
}