using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Tadmor
{
    public class TestModule : ModuleBase<ICommandContext>
    {
        [Command("synctest")]
        public async Task SyncTest()
        {
            await Task.Delay(1000);
        }

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("pong");
        }
    }
}