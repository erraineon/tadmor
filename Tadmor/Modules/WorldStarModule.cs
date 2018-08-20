using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Preconditions;
using Tadmor.Services.WorldStar;

namespace Tadmor.Modules
{
    public class WorldStarModule : ModuleBase<ICommandContext>
    {
        private readonly WorldStarService _worldStar;

        public WorldStarModule(WorldStarService worldStar)
        {
            _worldStar = worldStar;
        }

        [Command("wsh")]
        [Ratelimit(1, 1, Measure.Minutes, RatelimitFlags.ApplyPerGuild)]
        public async Task HotVideos()
        {
            var videos = await Task.WhenAll((await _worldStar.GetVideoInfosFromSite())
                .Where(v => !Regex.IsMatch(v.Title, "^.+ - .+$"))
                .OrderByDescending(v => v.Views)
                .Take(10)
                .Select(v => _worldStar.WithVideoUrl(v)));
            foreach (var video in videos)
                await ReplyAsync($"{video.Title} ({video.Views} views): {video.Url}");
        }
    }
}