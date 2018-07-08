using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;

namespace Tadmor.Modules
{
    [RequireOwner]
    public class DevModule : ModuleBase<ICommandContext>
    {
        [Command("ping")]
        public Task Ping() => ReplyAsync("pong");

        [Command("uptime")]
        public Task Uptime() => ReplyAsync((DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize());

        [Command("guilds")]
        public async Task Guilds()
        {
            var guilds = await Context.Client.GetGuildsAsync();
            await ReplyAsync(guilds.Humanize(g => $"{g.Name} ({g.Id})"));
        }

        [Command("leave")]
        public async Task LeaveGuild(ulong guildId)
        {
            var guild = await Context.Client.GetGuildAsync(guildId);
            if (guild != null) await guild.LeaveAsync();
        }

        [Command("say")]
        public Task Say([Remainder] string message) => ReplyAsync(message);
    }
}