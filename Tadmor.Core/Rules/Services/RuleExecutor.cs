using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Notifications;

namespace Tadmor.Core.Rules.Services
{
    public class RuleExecutor : IRuleExecutor
    {
        private readonly ICommandContextFactory _commandContextFactory;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ICommandResultPublisher _commandResultPublisher;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IRuleCommandParser _ruleCommandParser;

        public RuleExecutor(
            ICommandContextFactory commandContextFactory,
            IRuleCommandParser ruleCommandParser,
            ICommandExecutor commandExecutor,
            ICommandResultPublisher commandResultPublisher,
            INotificationPublisher notificationPublisher)
        {
            _commandContextFactory = commandContextFactory;
            _ruleCommandParser = ruleCommandParser;
            _commandExecutor = commandExecutor;
            _commandResultPublisher = commandResultPublisher;
            _notificationPublisher = notificationPublisher;
        }

        public async Task ExecuteRuleAsync(IRuleTriggerContext ruleTriggerContext, CancellationToken cancellationToken)
        {
            var command = await _ruleCommandParser.GetCommandAsync(ruleTriggerContext);
            var messageChannel = ruleTriggerContext.ExecuteIn;
            var commandContext = _commandContextFactory.Create(
                command,
                messageChannel,
                ruleTriggerContext.ExecuteAs,
                ruleTriggerContext.ChatClient,
                ruleTriggerContext.ReferencedMessage);

            var executeCommandRequest = new ExecuteCommandRequest(commandContext, command);
            var commandResult = await _commandExecutor.ExecuteAsync(executeCommandRequest, cancellationToken);
            if (commandResult.Error != CommandError.UnmetPrecondition)
            {
                var publishCommandResultRequest = new PublishCommandResultRequest(commandContext, commandResult);
                await _commandResultPublisher.PublishAsync(publishCommandResultRequest, cancellationToken);
            }

            var ruleExecutedNotification = new RuleExecutedNotification(
                ruleTriggerContext.ChatClient,
                messageChannel.Guild.Id,
                messageChannel.Id,
                ruleTriggerContext.Rule);
            await _notificationPublisher.PublishAsync(ruleExecutedNotification, cancellationToken);
        }
    }
}