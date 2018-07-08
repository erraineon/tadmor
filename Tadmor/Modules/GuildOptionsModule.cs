using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Tadmor.Services.Discord;

namespace Tadmor.Modules
{
    [RequireOwner(Group = "admin")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
    public class GuildOptionsModule : ModuleBase<ICommandContext>
    {
        private readonly DiscordOptions _discordOptions;

        public GuildOptionsModule(IOptionsSnapshot<DiscordOptions> discordOptions)
        {
            _discordOptions = discordOptions.Value;
        }

        [Command("prefix")]
        public async Task ChangePrefix(string newPrefix)
        {
            var guildId = Context.Guild.Id;
            var guildOptions = _discordOptions.GuildOptions.SingleOrDefault(options => options.Id == guildId);
            if (guildOptions == null)
            {
                guildOptions = new GuildOptions {Id = guildId};
                _discordOptions.GuildOptions.Add(guildOptions);
            }

            guildOptions.CommandPrefix = newPrefix;
            await Program.UpdateOptions(_discordOptions);
            await ReplyAsync("ok");
        }
    }
}