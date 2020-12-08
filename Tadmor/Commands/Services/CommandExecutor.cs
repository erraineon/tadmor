using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Commands.Interfaces;

namespace Tadmor.Commands.Services
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICommandService _commandService;

        public CommandExecutor(
            IServiceScopeFactory serviceScopeFactory,
            ICommandService commandService)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _commandService = commandService;
        }

        public async Task<IResult> BeginExecutionAsync(ICommandContext commandContext, string input)
        {
            var serviceScope = _serviceScopeFactory.CreateScope();
            var commandScope = new CommandScope(_commandService, commandContext, input, serviceScope);
            var result = await commandScope.BeginExecutionAsync();
            return result;
        }

        private record CommandScope(ICommandService CommandService, ICommandContext CommandContext, string Input, IServiceScope Scope)
        {
            public async Task<IResult> BeginExecutionAsync()
            {
                CommandService.CommandExecuted += DisposeScope;
                var result = await CommandService.ExecuteAsync(
                    CommandContext,
                    Input,
                    Scope.ServiceProvider);

                return result;
            }

            private Task DisposeScope(Optional<CommandInfo> _, ICommandContext completedContext, IResult __)
            {
                if (completedContext == CommandContext)
                {
                    CommandService.CommandExecuted -= DisposeScope;
                    Scope.Dispose();
                }

                return Task.CompletedTask;
            }
        }
    }
}