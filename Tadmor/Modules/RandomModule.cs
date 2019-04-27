using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using MoreLinq.Extensions;
using Tadmor.Extensions;
using Tadmor.Services.Data;
using Tadmor.Services.Tumblr;
using Tadmor.Services.Twitter;
using Tadmor.Services.WorldStar;

namespace Tadmor.Modules
{
    [Summary("rng")]
    public class RandomModule : ModuleBase<ICommandContext>
    {
        private static readonly Dictionary<(string, ulong channelId), (WorldStarVideo video, bool posted)> VideosCache =
            new Dictionary<(string, ulong), (WorldStarVideo, bool)>();
        private static readonly Random Random = new Random();
        private readonly TumblrService _tumblr;
        private readonly TwitterService _twitter;
        private readonly AppDbContext _context;

        private readonly WorldStarService _worldStar;
        public RandomModule(TwitterService twitter, TumblrService tumblr, WorldStarService worldStar, AppDbContext context)
        {
            _twitter = twitter;
            _tumblr = tumblr;
            _worldStar = worldStar;
            _context = context;
        }

        [Summary("roll a number between 1 and the specified one (defaults to 2)")]
        [Command("roll")]
        public Task Roll(int max = 2) => ReplyAsync((Random.Next(max) + 1).ToString());

        [Summary("pick a number of users, optionally in the specified group (defaults to one user)")]
        [Command("someone")]
        public async Task Someone(IRole role = default, int count = 1)
        {
            var users = (await Context.Channel.GetUsersAsync().FlattenAsync()).OfType<IGuildUser>();
            if (role != default) users = users.Where(u => u.RoleIds.Contains(role.Id));
            var mentions = users.RandomSubset(count).Select(u => u.Mention);
            await ReplyAsync(mentions.Humanize());
        }

        [Summary("pick a number of users who reacted to your last message with reactions")]
        [Command("someonereaction")]
        public async Task SomeoneReaction(int count = 1)
        {
            var lastMessages = await Context.Channel
                .GetMessagesAsync()
                .FlattenAsync();
            var lastMessageWithReactions = lastMessages
                .OfType<IUserMessage>()
                .FirstOrDefault(m => m.Author.Id == Context.User.Id &&
                                     m.Reactions.Any());
            if (lastMessageWithReactions == null)
                throw new Exception("none of your recent messages have reactions");
            var reactions = lastMessageWithReactions.Reactions;
            var usersByEmoji = await Task.WhenAll(reactions.Keys.Select(e => lastMessageWithReactions
                .GetReactionUsersAsync(e, int.MaxValue)
                .FlattenAsync()));
            var mentions = usersByEmoji
                .SelectMany(u => u)
                .DistinctBy(u => u.Id)
                .Where(u => u.Id != Context.User.Id)
                .RandomSubset(count)
                .Select(u => u.Mention);
            await ReplyAsync(mentions.Humanize());
        }

        [Summary("post a random food gore picture")]
        [Command("foodgore")]
        public Task FoodGore() => PostRandomTumblrPicture("someoneatethis");

        [Summary("post a random bad fursuit")]
        [Command("fursuit")]
        public Task Fursuit() => PostRandomTumblrPicture("horrificfursuits");

        [Summary("post a random image from the specified tumblr")]
        [Command("tumblr")]
        private async Task PostRandomTumblrPicture(string blogName)
        {
            var imageUrl = await _tumblr.GetRandomPost(blogName);
            var embed = new EmbedBuilder().WithImageUrl(imageUrl).Build();
            await ReplyAsync(string.Empty, embed: embed);
        }

        [Summary("post a random image from the specified twitter")]
        [Command("twitter")]
        public async Task RandomMedia(string username)
        {
            var statusUrl = await _twitter.GetRandomMediaStatusUrl(_context, username);
            await ReplyAsync(statusUrl);
        }

        [Summary("post a random video from world star hiphop")]
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
                var tuple = VideosCache.TryGetValue(key, out var v) ? v : default;
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
            var divisor = (float)pool.Max(v => v.Value.video.Views) - lowestViews;

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