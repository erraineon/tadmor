using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Options;
using MoreLinq.Extensions;
using Tadmor.Extensions;
using Tadmor.Preconditions;
using Tadmor.Services.Compliments;
using Tadmor.Services.Data;
using Tadmor.Services.Tumblr;
using Tadmor.Services.Twitter;

namespace Tadmor.Modules
{
    [Summary("rng")]
    public class RandomModule : ModuleBase<ICommandContext>
    {
        private static readonly Random Random = new Random();
        private readonly TumblrService _tumblr;
        private readonly TwitterService _twitter;
        private readonly AppDbContext _context;
        private readonly ComplimentsService _complimentsService;

        public RandomModule(TwitterService twitter, TumblrService tumblr, AppDbContext context, ComplimentsService complimentsService)
        {
            _twitter = twitter;
            _tumblr = tumblr;
            _context = context;
            _complimentsService = complimentsService;
        }

        [Summary("roll a number between 1 and the specified one (defaults to 2)")]
        [Command("roll")]
        public Task Roll(int max = 2) => ReplyAsync((Random.Next(max) + 1).ToString());

        [Summary("pick a number of users who reacted to your last message with reactions")]
        [Command("someonereaction")]
        public async Task SomeoneReaction(int count = 1)
        {
            var lastMessageWithReactions = await Context.Channel
                .GetMessagesAsync()
                .Flatten()
                .OfType<IUserMessage>()
                .FirstOrDefaultAsync(m => m.Author.Id == Context.User.Id &&
                                     m.Reactions.Any());
            if (lastMessageWithReactions == null)
                throw new Exception("none of your recent messages have reactions");
            var reactions = lastMessageWithReactions.Reactions;
            var mentions = (await reactions.Keys
                    .Select(e => lastMessageWithReactions.GetReactionUsersAsync(e, int.MaxValue))
                    .ToAsyncEnumerable()
                    .SelectMany(g => g)
                    .FlattenAsync())
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

        [Summary("pick a random option among the supplied ones")]
        [Command("pick")]
        public async Task Pick(params string[] options)
        {
            if (options.Distinct().Count() < 2) throw new Exception("need at least two options");
            var option = options.RandomSubset(1).Single();
            await ReplyAsync(option);
        }

        [Summary("flirts with the specified user, or the sender")]
        [Command("flirt")]
        public async Task Flirt(IGuildUser? user = null)
        {
            user ??= (IGuildUser) Context.User;
            var compliment = _complimentsService.Compliments.RandomSubset(1).Single();
            await ReplyAsync($"{user.Mention}: {compliment}");
        }
    }
}