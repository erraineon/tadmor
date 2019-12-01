using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Wolfram;

namespace Tadmor.Modules
{
    [Summary("wolfram")]
    public class WolframModule : ModuleBase<ICommandContext>
    {
        private readonly WolframService _wolfram;

        public WolframModule(WolframService wolfram)
        {
            _wolfram = wolfram;
        }

        [Summary("queries wolfram alpha")]
        [Command("wa")]
        public async Task Query([Remainder] string query)
        {
            var pods = await _wolfram.Query(query);
            var orderedPods = pods
                .OrderByDescending(p => p.Primary)
                .ToList();
            var embedBuilder = new EmbedBuilder();
            var (chosenPod, subPods) = orderedPods
                .Select(p => (p, subPods: p.SubPods.Where(subPod => !string.IsNullOrEmpty(subPod.Plaintext)).ToList()))
                .FirstOrDefault(t => t.subPods.Any());
            if (chosenPod != null)
            {
                foreach (var subPod in subPods)
                {
                    var title = !string.IsNullOrEmpty(subPod.Title)
                        ? subPod.Title
                        : !string.IsNullOrEmpty(chosenPod.Title)
                            ? chosenPod.Title
                            : "result";
                    embedBuilder.AddField(title, subPod.Plaintext);
                }
            }

            var bestImageCandidate = orderedPods
                .SelectMany(p => p.SubPods, (_, subPod) => subPod.Image)
                .FirstOrDefault(i => string.IsNullOrEmpty(i.Title) && !string.IsNullOrEmpty(i.Src));
            if (bestImageCandidate != null)
            {
                var imageUrl = bestImageCandidate.Src.Replace("image/gif", "image/png");
                embedBuilder.WithImageUrl(imageUrl);
            }

            await ReplyAsync(string.Empty, embed: embedBuilder.Build());
        }
    }
}