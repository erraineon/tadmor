using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Services
{
    public class LoggingCommandExecutorDecorator : ICommandExecutor
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly ILogger<LoggingCommandExecutorDecorator> _logger;

        public LoggingCommandExecutorDecorator(ICommandExecutor commandExecutor, ILogger<LoggingCommandExecutorDecorator> logger)
        {
            _commandExecutor = commandExecutor;
            _logger = logger;
        }

        public async Task<IResult> ExecuteAsync(ExecuteCommandRequest request, CancellationToken cancellationToken)
        {
            var result = await _commandExecutor.ExecuteAsync(request, cancellationToken);
            if (result is ExecuteResult {Exception: { } e})
                _logger.LogError(e, $"unhandled exception in {request.Input}");
            return result;
        }
    }
}