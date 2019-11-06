using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Services.Options;

namespace Tadmor.Preconditions
{
    public class RequireNoGoodBoyModeAttribute : RequireNsfwAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var chatOptions = services.GetService<ChatOptionsService>();
            var writableOptions = chatOptions.GetOptions();
            var guildOptions = chatOptions.GetGuildOptions(context.Guild.Id, writableOptions.Value);
            return guildOptions.GoodBoyMode
                ? base.CheckPermissionsAsync(context, command, services)
                : Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}