using System.Threading.Tasks;
using Discord.Commands;

namespace Tadmor.Modules
{
    public class DevModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task Ping() => ReplyAsync("pong");
    }
}