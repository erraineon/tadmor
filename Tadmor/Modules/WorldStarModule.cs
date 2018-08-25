using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.WorldStar;

namespace Tadmor.Modules
{
    public class WorldStarModule : ModuleBase<ICommandContext>
    {
        private static readonly Dictionary<(string, ulong channelId), (WorldStarVideo video, bool posted)> VideosCache =
            new Dictionary<(string, ulong), (WorldStarVideo, bool)>();

        private readonly WorldStarService _worldStar;

        public WorldStarModule(WorldStarService worldStar)
        {
            _worldStar = worldStar;
        }

        [Command("wsh")]
        public async Task RandomVideo()
        {
            var videos = (await _worldStar.GetVideoInfosFromSite())
                .Where(v => !Regex.IsMatch(v.Title, "^.+ - .+$"))
                .ToList();
            var channelId = Context.Channel.Id;
            foreach (var video in videos)
            {
                var key = (video.PageUrl, channelId);
                var tuple = !VideosCache.TryGetValue(key, out var v) ? v : default;
                tuple.video = video;
                VideosCache[key] = tuple;
            }

            var cutoffDate = videos.Min(v => v.PostedOn);
            var oldVideos = VideosCache
                .Where(pair => pair.Value.video.PostedOn < cutoffDate)
                .Select(pair => pair.Key)
                .ToList();
            foreach (var key in oldVideos) VideosCache.Remove(key);
            var pool = VideosCache
                .Where(pair => pair.Key.channelId == channelId && !pair.Value.posted)
                .ToList();
            if (!pool.Any()) throw new Exception("all the recent videos have been posted");
            var lowestViews = pool.Min(v => v.Value.video.Views);
            var divisor = (float) pool.Max(v => v.Value.video.Views) - lowestViews;

            //weight will be between 1 and 4 based on views
            float GetWeight(KeyValuePair<(string, ulong channelId), (WorldStarVideo video, bool posted)> pair)
            {
                return 1 + (pair.Value.video.Views - lowestViews) * 3 / divisor;
            }

            var chosenPair = pool.Random(GetWeight, new Random());
            var toPost = await _worldStar.WithVideoUrl(chosenPair.Value.video);
            await ReplyAsync($"{toPost.Title} ({toPost.Views} views): {toPost.Url}");
            VideosCache[chosenPair.Key] = (toPost, true);
        }
    }
}