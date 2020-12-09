using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Commands.Interfaces;

namespace Tadmor.Commands.Services
{
    [ExcludeFromCodeCoverage]
    public class CommandServiceWrapper : ICommandService
    {
        public Task<bool> RemoveModuleAsync(ModuleInfo module)
        {
            return _commandService.RemoveModuleAsync(module);
        }

        public IEnumerable<ModuleInfo> Modules => _commandService.Modules;

        public event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted
        {
            add => _commandService.CommandExecuted += value;
            remove => _commandService.CommandExecuted -= value;
        }

        public Task<ModuleInfo> AddModuleAsync(Type type, IServiceProvider services)
        {
            return _commandService.AddModuleAsync(type, services);
        }

        public Task<IResult> ExecuteAsync(ICommandContext context, string input, IServiceProvider services,
            MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
        {
            return _commandService.ExecuteAsync(context, input, services, multiMatchHandling);
        }

        private readonly CommandService _commandService;

        public CommandServiceWrapper(CommandService commandService)
        {
            _commandService = commandService;
        }
    }
}