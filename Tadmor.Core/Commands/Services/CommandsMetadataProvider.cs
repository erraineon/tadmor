using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Interfaces;

namespace Tadmor.Core.Commands.Services
{
    public class CommandsMetadataProvider : ICommandsMetadataProvider
    {
        private readonly ICommandService _commandService;
        private readonly ICommandPermissionValidator _commandPermissionValidator;
        private readonly IServiceProvider _serviceProvider;

        public CommandsMetadataProvider(ICommandService commandService,
            ICommandPermissionValidator commandPermissionValidator, 
            IServiceProvider serviceProvider)
        {
            _commandService = commandService;
            _commandPermissionValidator = commandPermissionValidator;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<IGrouping<ModuleInfo, CommandInfo>>> GetCommandsByModuleAsync(ICommandContext commandContext)
        {
            var availableCommands = await GetCommandsAsync(commandContext);
            var availableCommandsByModule = availableCommands
                .GroupBy(commandInfo =>
                {
                    var module = commandInfo.Module;
                    while (module.Parent != null) module = module.Parent;
                    return module;
                })
                .OrderBy(g => g.Key.Summary ?? g.Key.Name);
            return availableCommandsByModule;
        }

        public async Task<IEnumerable<CommandInfo>> GetCommandsAsync(ICommandContext commandContext)
        {
            var commandsAndAvailabilities = await Task.WhenAll(_commandService.Commands
                .Select(async commandInfo =>
                {
                    var isAvailable = await _commandPermissionValidator.CanRunAsync(commandContext, commandInfo) &&
                                      (await commandInfo.CheckPreconditionsAsync(commandContext, _serviceProvider)).IsSuccess;
                    return (commandInfo, isAvailable);
                }));
            var availableCommands = commandsAndAvailabilities
                .Where(t => t.isAvailable)
                .Select(t => t.commandInfo);
            return availableCommands;
        }
    }
}