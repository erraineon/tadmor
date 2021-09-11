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
        private readonly ITadmorMindClient _tadmorMindClient;

        public TadmorMindModule(
            ITadmorMindThoughtsRepository tadmorMindThoughtsRepository,
            ITadmorMindClient tadmorMindClient)
        {
            _tadmorMindThoughtsRepository = tadmorMindThoughtsRepository;
            _tadmorMindClient = tadmorMindClient;
        }

        [Summary("generate a thought")]
        [Command("gen")]
        [RequireWhitelist]
        public async Task<RuntimeResult> GenerateThoughtAsync()
        {
            var generatedText = await _tadmorMindThoughtsRepository.ReceiveAsync();
            return CommandResult.FromSuccess(generatedText);
        }

        [Summary("complete a thought")]
        [Command("gen")]
        [RequireWhitelist]
        public async Task<RuntimeResult> GenerateCompletionAsync([Remainder]string prompt)
        {
            if (prompt.Length >= 128) throw new ModuleException("the prompt can be up to 128 characters long");
            var generatedText = await _tadmorMindClient.GenerateCompletionAsync(prompt);
            return CommandResult.FromSuccess(generatedText);
        }
    }
}