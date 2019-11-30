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
            var (pod, subPods) = orderedPods
                .Select(p => (p, subPods: p.SubPods.Where(subPod => !string.IsNullOrEmpty(subPod.Plaintext)).ToList()))
                .FirstOrDefault(t => t.subPods.Any());
            if (pod != null)
            {
                foreach (var subPod in subPods)
                {
                    var title = !string.IsNullOrEmpty(subPod.Title)
                        ? subPod.Title
                        : !string.IsNullOrEmpty(pod.Title)
                            ? pod.Title
                            : "result";
                    embedBuilder.AddField(title, subPod.Plaintext);
                }
            }

            var bestImageCandidate = orderedPods
                .SelectMany(p => p.SubPods, (_, subPod) => subPod.Image)
                .FirstOrDefault(i => string.IsNullOrEmpty(i.Title) && !string.IsNullOrEmpty(i.Src));
            if (bestImageCandidate != null) embedBuilder.WithImageUrl(bestImageCandidate.Src.Replace("image/gif", "image/png"));

            await ReplyAsync(string.Empty, embed: embedBuilder.Build());
        }
    }
}