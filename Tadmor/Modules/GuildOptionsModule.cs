using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Discord;

namespace Tadmor.Modules
{
    [RequireOwner(Group = "admin")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
    public class GuildOptionsModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordService _discordService;

        public GuildOptionsModule(DiscordService discordService)
        {
            _discordService = discordService;
        }

        [Command("prefix")]
        public async Task ChangePrefix(string newPrefix)
        {
            await _discordService.ChangeCommandPrefix(Context.Guild, newPrefix);
            await ReplyAsync("ok");
        }
    }
}