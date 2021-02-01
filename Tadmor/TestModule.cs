using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Commands.Attributes;
using Tadmor.Commands.Models;

namespace Tadmor
{
    public class TestModule : ModuleBase<ICommandContext>
    {
        [Command("synctest")]
        public async Task<RuntimeResult> SyncTest()
        {
            await Task.Delay(1000);
            return CommandResult.FromSuccess("sync test pass");
        }

        [Command("ping")]
        public Task<RuntimeResult> Ping() => Task.FromResult(CommandResult.FromSuccess("pong"));

        [Command("throw")]
        public async Task Throw() => throw new Exception("error");

        [Command("throwp")]
        public async Task ThrowPublicFacing() => throw new FrontEndException("error");

        [Command("echo")]
        public Task<RuntimeResult> Echo([Remainder] string value) => Task.FromResult(CommandResult.FromSuccess(value));

        [RequireWhitelist]
        [Command("wlist")]
        public Task<RuntimeResult> Whitelist() => Task.FromResult(CommandResult.FromSuccess("whitelist success"));
    }
}