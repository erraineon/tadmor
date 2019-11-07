using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Services.Commands;
using Tadmor.Services.Options;

namespace Tadmor.Preconditions
{
    public class RequireWhitelistAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var chatOptions = services.GetService<ChatOptionsService>();
            var commandName = command.Name;
            var permissions = chatOptions.GetPermissions(commandName);
            var whitelisted = permissions
                .Where(p => p.PermissionType == PermissionType.Whitelist)
                .Any(p => p.ScopeType switch
                {
                    CommandUsagePermissionScopeType.User => context.User.Id == p.ScopeId,
                    CommandUsagePermissionScopeType.Channel => context.Channel.Id == p.ScopeId,
                    CommandUsagePermissionScopeType.Guild => context.Guild.Id == p.ScopeId,
                    _ => false
                });
            return Task.FromResult(whitelisted
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("command requires whitelisting"));
        }
    }
}