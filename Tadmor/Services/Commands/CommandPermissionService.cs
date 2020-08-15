using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tadmor.Services.Options;

namespace Tadmor.Services.Commands
{
    [ScopedService]
    public class CommandPermissionService
    {
        private readonly IServiceProvider _services;
        private readonly ChatOptionsService _chatOptions;

        public CommandPermissionService(IServiceProvider services, ChatOptionsService chatOptions)
        {
            _services = services;
            _chatOptions = chatOptions;
        }
        public async Task<bool> CanExecute(ICommandContext context, CommandInfo command)
        {
            var passesPreconditions = await command.CheckPreconditionsAsync(context, _services);
            if (passesPreconditions.IsSuccess)
            {
                var commandName = command.Name;
                var permissions = _chatOptions.GetPermissions(commandName);
                var whitelisted = permissions
                    .Where(p => p.PermissionType != PermissionType.None)
                    .Any(p => p.ScopeType switch
                    {
                        CommandUsagePermissionScopeType.User => context.User.Id == p.ScopeId,
                        CommandUsagePermissionScopeType.Channel => context.Channel.Id == p.ScopeId,
                        CommandUsagePermissionScopeType.Guild => context.Guild.Id == p.ScopeId,
                        _ => false
                    });
            }
            return true;
        }
    }
}
