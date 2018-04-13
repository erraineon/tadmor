using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Tadmor.Modules
{
    public class RandomModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Random Random = new Random();

        [Command("roll")]
        public Task Roll(int max = 1) => ReplyAsync(Random.Next(max + 1).ToString());
    }
}