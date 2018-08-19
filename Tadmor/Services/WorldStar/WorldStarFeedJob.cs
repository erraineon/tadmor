using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Tadmor.Services.Cron;
using Tadmor.Extensions;

namespace Tadmor.Services.WorldStar
{
    public class WorldStarFeedJob : ICronJob<WorldStarFeedJobOptions>
    {
        private readonly DiscordSocketClient _discord;
        private readonly WorldStarService _worldStar;

        public WorldStarFeedJob(DiscordSocketClient discord, WorldStarService worldStar)
        {
            _discord = discord;
            _worldStar = worldStar;
        }

        [UpdateArguments]
        [CancelRecurrenceUponFailure]
        public async Task Do(WorldStarFeedJobOptions options)
        {
            var destChannel = _discord.GetChannel(options.ChannelId) as IMessageChannel ??
                              throw new Exception("channel gone, delete schedule");
            var videos = await _worldStar.GetVideoInfos();
            var lastIndex = videos.FindIndex(v => v.PageUrl == options.LastLink);
            var newVideos = await Task.WhenAll(videos
                .Take(lastIndex >= 0 ? lastIndex : 1)
                .Select(v => _worldStar.WithVideoUrl(v)));
            if (newVideos.Any())
            {
                options.LastLink = newVideos.First().Url;
                foreach (var newVideo in newVideos)
                    await destChannel.SendMessageAsync($"new video: {newVideo.Title} {newVideo.Url}");
            }
        }
    }
}