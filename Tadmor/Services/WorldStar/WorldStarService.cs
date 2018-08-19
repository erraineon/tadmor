using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeHollow.FeedReader;

namespace Tadmor.Services.WorldStar
{
    public class WorldStarService
    {
        private static readonly HttpClient Client = new HttpClient();

        public async Task<List<WorldStarVideo>> GetVideoInfos()
        {
            var feed = await FeedReader.ReadAsync("http://www.worldstarhiphop.com/videos/rss.php");
            var urls = feed.Items
                .Select(i => new WorldStarVideo
                {
                    PageUrl = i.Link,
                    Title = i.Title
                })
                .ToList();
            await (WithVideoUrl(urls[0]));
            return urls;
        }

        public async Task<WorldStarVideo> WithVideoUrl(WorldStarVideo video)
        {
            var html = await Client.GetStringAsync(video.PageUrl);
            //no html parsing because it's malformed
            var videoUrl =
                Regex.Match(html, @"[^""]+?(?="" type=""video/mp4)") is var onSiteMatch && onSiteMatch.Success
                    ? onSiteMatch.Value
                    : Regex.Match(html, @"(?<=youtube\.com/embed/).+?(?=\?)") is var ytMatch && ytMatch.Success
                        ? $"https://youtube.com/watch?v={ytMatch.Value}"
                        : string.Empty;
            video.Url = videoUrl;
            return video;
        }
    }
}