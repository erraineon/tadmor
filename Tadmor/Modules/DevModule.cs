using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;

namespace Tadmor.Modules
{
    [RequireOwner]
    public class DevModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task Ping() => ReplyAsync("pong");

        [Command("uptime")]
        public Task Uptime() => ReplyAsync((DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize());

        [Command("guilds")]
        public Task Guilds() => ReplyAsync(Context.Client.Guilds.Humanize(g => $"{g.Name} ({g.Id})"));

        [Command("leave")]
        public Task LeaveGuild(ulong guildId) => Context.Client.GetGuild(guildId)?.LeaveAsync();
    }
}