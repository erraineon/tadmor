using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;
using Tadmor.Notifications.Interfaces;

namespace Tadmor.Commands.Services
{
    public class ChatCommandMonitor : INotificationHandler<MessageValidatedNotification>
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly ICommandPermissionValidator _commandPermissionValidator;
        private readonly ICommandPrefixValidator _commandPrefixValidator;
        private readonly ICommandResultPublisher _commandResultPublisher;

        public ChatCommandMonitor(
            ICommandPrefixValidator commandPrefixValidator,
            ICommandExecutor commandExecutor,
            ICommandResultPublisher commandResultPublisher,
            ICommandPermissionValidator commandPermissionValidator)
        {
            _commandPrefixValidator = commandPrefixValidator;
            _commandPermissionValidator = commandPermissionValidator;
            _commandExecutor = commandExecutor;
            _commandResultPublisher = commandResultPublisher;
        }

        public async Task HandleAsync(MessageValidatedNotification notification, CancellationToken cancellationToken)
        {
            var prefixValidationResult = await _commandPrefixValidator.ValidatePrefix(notification, cancellationToken);
            if (prefixValidationResult.IsPrefixValid)
            {
                var commandContext = new CommandContext(notification.ChatClient, notification.UserMessage);
                var executeCommandRequest = new ExecuteCommandRequest(commandContext, prefixValidationResult.Input);
                if (await _commandPermissionValidator.CanRunAsync(executeCommandRequest, cancellationToken))
                {
                    var result = await _commandExecutor.ExecuteAsync(executeCommandRequest, cancellationToken);
                    await _commandResultPublisher.PublishAsync(
                        new PublishCommandResultRequest(commandContext, result),
                        cancellationToken);
                }
                else
                {
                    await commandContext.Channel.SendMessageAsync("no");
                }
            }
        }
    }
}