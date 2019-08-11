using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tadmor.Services.Discord;

namespace Tadmor.Preconditions
{
    public class RequireNoGoodBoyModeAttribute : RequireNsfwAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var discordOptions = scope.ServiceProvider.GetService<IOptionsSnapshot<DiscordOptions>>().Value;
                var guildId = context.Guild.Id;
                var guildOptions = discordOptions.GuildOptions.SingleOrDefault(options => options.Id == guildId);
                return guildOptions?.GoodBoyMode == true
                    ? base.CheckPermissionsAsync(context, command, services)
                    : Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }
}