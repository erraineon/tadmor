using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Services
{
    public class PermissionAwareCommandExecutorDecorator : ICommandExecutor
    {
        private readonly ICommandPermissionValidator _commandPermissionValidator;
        private readonly ICommandExecutor _commandExecutor;
        public PermissionAwareCommandExecutorDecorator(
            ICommandPermissionValidator commandPermissionValidator, 
            ICommandExecutor commandExecutor)
        {
            _commandPermissionValidator = commandPermissionValidator;
            _commandExecutor = commandExecutor;
        }

        public async Task<IResult> ExecuteAsync(ExecuteCommandRequest request, CancellationToken cancellationToken)
        {
            var canRun = await _commandPermissionValidator.CanRunAsync(request, cancellationToken);
            var result = canRun != true
                ? ExecuteResult.FromError(CommandError.UnmetPrecondition,
                    "you don't have permission to run the command")
                : await _commandExecutor.ExecuteAsync(request, cancellationToken);
            return result;
        }


    }
}