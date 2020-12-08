using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Services.Options;

namespace Tadmor.Services.Commands
{
    [SingletonService]
    public class CommandPermissionService
    {
        private readonly ChatOptionsService _chatOptions;
        private readonly IServiceProvider _services;

        public CommandPermissionService(IServiceProvider services, ChatOptionsService chatOptions)
        {
            _services = services;
            _chatOptions = chatOptions;
        }

        public async Task<bool> PassesLists(ICommandContext context, CommandInfo command)
        {
            var canExecute = PassesWhitelist(context, command) && PassesBlacklist(context, command);
            return canExecute;
        }

        private bool PassesWhitelist(ICommandContext context, CommandInfo command)
        {
            var isAllowed = !command.Attributes.Any(c => c is RequireWhitelistAttribute) ||
                GetPermissionsForCommandAndWildcard(command, PermissionType.Whitelist)
                    .Any(p => ContextMatchesPermission(context, p));
            return isAllowed;
        }

        private bool PassesBlacklist(ICommandContext context, CommandInfo command)
        {
            var permissions = GetPermissionsForCommandAndWildcard(command, PermissionType.Blacklist);
            var isAllowed = permissions
                .All(p => !ContextMatchesPermission(context, p));
            return isAllowed;
        }

        private IEnumerable<CommandUsagePermission> GetPermissionsForCommandAndWildcard(
            CommandInfo command,
            PermissionType permissionType)
        {
            var permissions = _chatOptions.GetPermissions(command.Name)
                .Concat(_chatOptions.GetPermissions("*"))
                .Where(p => p.PermissionType == permissionType);
            return permissions;
        }

        private static bool ContextMatchesPermission(ICommandContext context, CommandUsagePermission p)
        {
            var contextMatchesPermission = p.ScopeType switch
            {
                CommandUsagePermissionScopeType.User => context.User.Id == p.ScopeId,
                CommandUsagePermissionScopeType.Channel => context.Channel.Id == p.ScopeId,
                CommandUsagePermissionScopeType.Guild => context.Guild.Id == p.ScopeId,
                _ => false
            };
            return contextMatchesPermission;
        }
    }
}