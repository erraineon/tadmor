using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Models;
using Tadmor.TextGeneration.Interfaces;
using Tadmor.TextGeneration.Services;

namespace Tadmor.TextGeneration.Modules
{
    [Summary("let him talk")]
    public class TadmorMindModule : ModuleBase<ICommandContext>
    {
        private readonly ITadmorMindThoughtsRepository _tadmorMindThoughtsRepository;

        public TadmorMindModule(ITadmorMindThoughtsRepository tadmorMindThoughtsRepository)
        {
            _tadmorMindThoughtsRepository = tadmorMindThoughtsRepository;
        }

        [Summary("generate a thought")]
        [Command("gen")]
        [RequireWhitelist]
        public async Task<RuntimeResult> GenerateThoughtAsync()
        {
            var generatedText = await _tadmorMindThoughtsRepository.ReceiveAsync();
            return CommandResult.FromSuccess(generatedText);
        }
    }
}