using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Models;

namespace Tadmor
{
    [HideInHelp]
    public class TestModule : ModuleBase<ICommandContext>
    {
        [Command("synctest")]
        public async Task<RuntimeResult> SyncTest()
        {
            await Task.Delay(1000);
            return CommandResult.FromSuccess("sync test pass");
        }

        [Command("ping")]
        public Task<RuntimeResult> Ping()
        {
            return Task.FromResult(CommandResult.FromSuccess("pong"));
        }

        [Command("reply")]
        public Task<RuntimeResult> Reply()
        {
            return Task.FromResult(CommandResult.FromSuccess("replied", true));
        }

        [Command("throw")]
        public async Task Throw()
        {
            throw new Exception("inner error");
        }

        [Command("throwp")]
        public async Task ThrowPublicFacing()
        {
            throw new ModuleException("public facing error");
        }

        [Command("echo")]
        public Task<RuntimeResult> Echo([Remainder] string value)
        {
            return Task.FromResult(CommandResult.FromSuccess(value));
        }

        [RequireWhitelist]
        [Command("wlist")]
        public Task<RuntimeResult> Whitelist()
        {
            return Task.FromResult(CommandResult.FromSuccess("whitelist success"));
        }

        [Command("del")]
        public async Task<RuntimeResult?> Delete([Remainder] string message)
        {
            if (Context.Message.ReferencedMessage is { } messageToDelete)
                await messageToDelete.DeleteAsync();
            return !string.IsNullOrWhiteSpace(message) ? CommandResult.FromSuccess(message) : default;
        }
    }
}