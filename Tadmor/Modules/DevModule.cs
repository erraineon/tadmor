using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;

namespace Tadmor.Modules
{
    public class DevModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task Ping() => ReplyAsync("pong");

        [Command("uptime")]
        public Task Uptime() => ReplyAsync((DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize());
    }
}