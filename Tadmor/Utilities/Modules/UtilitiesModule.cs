using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Extensions;

namespace Tadmor.Utilities.Modules
{
    [Summary("utilities")]
    public class UtilitiesModule : ModuleBase<ICommandContext>
    {
        private static readonly Random Random = new();

        [Summary("flips a coin")]
        [Command("flip")]
        public Task<RuntimeResult> Flip()
        {
            return Pick("heads", "tails");
        }

        [Summary("roll a number between 1 and the specified one (defaults to 6)")]
        [Command("roll")]
        public async Task<RuntimeResult> Roll(int max = 6)
        {
            return CommandResult.FromSuccess((Random.Next(max) + 1).ToString(), true);
        }

        [Summary("pick a random option among the supplied ones")]
        [Command("pick")]
        public async Task<RuntimeResult> Pick(params string[] options)
        {
            if (options.Distinct().Count() < 2) throw new ModuleException("need at least two options");
            var option = options.Random();
            return CommandResult.FromSuccess(option, true);
        }
    }
}