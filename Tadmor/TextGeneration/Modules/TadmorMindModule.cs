using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Models;
using Tadmor.TextGeneration.Interfaces;

namespace Tadmor.TextGeneration.Modules
{
    [Summary("let him talk")]
    public class TadmorMindModule : ModuleBase<ICommandContext>
    {
        private readonly ITadmorMindClient _tadmorMindClient;

        public TadmorMindModule(ITadmorMindClient tadmorMindClient)
        {
            _tadmorMindClient = tadmorMindClient;
        }

        [Summary("generate a thought")]
        [Command("gen")]
        [RequireWhitelist]
        public async Task<RuntimeResult> GenerateThoughtAsync()
        {
            var generatedText = await _tadmorMindClient.GenerateAsync();
            return CommandResult.FromSuccess(generatedText);
        }
    }
}