using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;

namespace Tadmor.Commands.Services
{
    public class LoggingCommandExecutor : ICommandExecutor
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly ILogger<LoggingCommandExecutor> _logger;

        public LoggingCommandExecutor(ICommandExecutor commandExecutor, ILogger<LoggingCommandExecutor> logger)
        {
            _commandExecutor = commandExecutor;
            _logger = logger;
        }
        public async Task<IResult> ExecuteAsync(ExecuteCommandRequest request, CancellationToken cancellationToken)
        {
            var result = await _commandExecutor.ExecuteAsync(request, cancellationToken);
            if (result is ExecuteResult { Exception: { } e })
                _logger.LogError(e, $"unhandled exception in {request.Input}");
            return result;
        }
    }
}