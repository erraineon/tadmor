using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using Tadmor.Extensions;
using Tadmor.Utils;

namespace Tadmor.Services.WorldStar
{
    public class WorldStarService
    {
        private const string BaseUrl = "http://worldstarhiphop.com";
        private static readonly HttpClient Client = new HttpClient();


        public async Task<List<WorldStarVideo>> GetVideoInfosFromSite()
        {
            var htmlStream = await Client.GetStreamAsync($"{BaseUrl}/videos/");
            var doc = new HtmlDocument();
            doc.Load(htmlStream);
            var videosByDate = doc.DocumentNode
                .SelectNodes("//section[@class='videos']/section[@class='box' and @itemscope]")
                .Select(video => new WorldStarVideo
                {
                    PageUrl = $"{BaseUrl}{video.SelectSingleNode("a").GetAttributeValue("href", string.Empty)}",
                    Title = StringUtils.StripHtml(video.SelectSingleNode("strong[@class='title']/a").InnerText),
                    Views = int.Parse(
                        video.SelectSingleNode("*//span[@class='views']").InnerText,
                        NumberStyles.AllowThousands),
                    PostedOn = DateTime.Parse(video.ParentNode.ParentNode
                        .SelectSingleNode("*//time")
                        .GetAttributeValue("datetime", string.Empty))
                })
                .ToList();
            return videosByDate;
        }

        public async Task<List<WorldStarVideo>> GetVideoInfosFromRss()
        {
            var feed = await FeedReader.ReadAsync("http://www.worldstarhiphop.com/videos/rss.php");
            var infos = feed.Items
                .Select(i => new WorldStarVideo
                {
                    PageUrl = i.Link,
                    Title = i.Title
                })
                .ToList();
            return infos;
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