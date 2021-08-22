using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Services
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ICommandResultPublisher _commandResultPublisher;
        private readonly ICommandService _commandService;
        private readonly ICommandServiceScopeFactory _commandServiceScopeFactory;

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
    }
}