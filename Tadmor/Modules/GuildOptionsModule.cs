using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Tadmor.Services.Discord;

namespace Tadmor.Modules
{
    [Summary("guild options")]
    [RequireOwner(Group = "admin")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
    public class GuildOptionsModule : ModuleBase<ICommandContext>
    {
        private readonly DiscordOptions _discordOptions;

        public GuildOptionsModule(IOptionsSnapshot<DiscordOptions> discordOptions)
        {
            _discordOptions = discordOptions.Value;
        }

        [Summary("change the prefix for commands on this guild")]
        [Command("prefix")]
        public async Task ChangePrefix(string newPrefix)
        {
            var guildId = Context.Guild.Id;
            var guildOptions = GetOrAddOptions(guildId);
            guildOptions.CommandPrefix = newPrefix;
            await Program.UpdateOptions(_discordOptions);
            await ReplyAsync("ok");
        }

        [Summary("change the welcome message for this guild")]
        [Command("welcome")]
        public async Task ChangeWelcomeMessage([Remainder] string newWelcomeMessage = default)
        {
            var guildId = Context.Guild.Id;
            var guildOptions = GetOrAddOptions(guildId);
            guildOptions.WelcomeMessage = newWelcomeMessage;
            guildOptions.WelcomeChannel = Context.Channel.Id;
            await Program.UpdateOptions(_discordOptions);
            await ReplyAsync("ok");
        }

        private GuildOptions GetOrAddOptions(ulong guildId)
        {
            var guildOptions = _discordOptions.GuildOptions.SingleOrDefault(options => options.Id == guildId);
            if (guildOptions == null)
            {
                guildOptions = new GuildOptions {Id = guildId};
                _discordOptions.GuildOptions.Add(guildOptions);
            }

            return guildOptions;
        }
    }
}