using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;

namespace Tadmor.Commands.Services
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ICommandServiceScopeFactory _commandServiceScopeFactory;
        private readonly ICommandService _commandService;
        private readonly ICommandResultPublisher _commandResultPublisher;

        public CommandExecutor(
            ICommandServiceScopeFactory commandServiceScopeFactory,
            ICommandService commandService,
            ICommandResultPublisher commandResultPublisher)
        {
            _commandServiceScopeFactory = commandServiceScopeFactory;
            _commandService = commandService;
            _commandResultPublisher = commandResultPublisher;
        }

        public async Task<IResult> ExecuteAsync(ExecuteCommandRequest request, CancellationToken cancellationToken)
        {
            var (commandContext, input) = request;
            using var serviceScope = await _commandServiceScopeFactory.CreateScopeAsync(commandContext);
            var result = await _commandService.ExecuteAsync(commandContext, input, serviceScope.ServiceProvider);
            return result;
        }

        public async Task ExecuteAndPublishAsync(ExecuteCommandRequest request, CancellationToken cancellationToken)
        {
            var result = await ExecuteAsync(request, cancellationToken);
            var publishCommandResultRequest = new PublishCommandResultRequest(request.CommandContext, result);
            await _commandResultPublisher.PublishAsync(publishCommandResultRequest, cancellationToken);
        }
    }
}