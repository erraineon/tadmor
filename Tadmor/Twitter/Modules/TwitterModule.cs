﻿using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Extensions;
using Tadmor.MessageRendering.Interfaces;
using Tadmor.Twitter.Interfaces;

namespace Tadmor.Twitter.Modules
{
    [Summary("twitter")]
    public class TwitterModule : ModuleBase<ICommandContext>
    {
        private readonly IRandomTweetProvider _randomTweetProvider;
        private readonly IImageTweetSender _imageTweetSender;
        private readonly IDrawableMessageFactory _drawableMessageFactory;
        private readonly IMessageRenderer _messageRenderer;

        public TwitterModule(IRandomTweetProvider randomTweetProvider,
            IImageTweetSender imageTweetSender,
            IDrawableMessageFactory drawableMessageFactory,
            IMessageRenderer messageRenderer)
        {
            _randomTweetProvider = randomTweetProvider;
            _imageTweetSender = imageTweetSender;
            _drawableMessageFactory = drawableMessageFactory;
            _messageRenderer = messageRenderer;
        }

        [Command("twitter")]
        [Summary("posts a random tweet from the specified user")]
        public async Task<RuntimeResult> GetRandomTweetAsync(string displayName, [Remainder] string? filter = default)
        {
            return await GetRandomTweetAsync(displayName, filter, false);
        }

        [Command("twitter media")]
        [Summary("posts a random media tweet from the specified user")]
        [Priority(1)]
        public async Task<RuntimeResult> GetRandomMediaTweetAsync(string displayName, [Remainder] string? filter = default)
        {
            return await GetRandomTweetAsync(displayName, filter, true);
        }

        [Command("tweet")]
        [Summary("tweets the specified number of messages, starting from the last message or the one being replied to")]
        [RequireWhitelist]
        public async Task<RuntimeResult> TweetAsync(int messagesToTweet = 1)
        {
            var drawableMessages = await Context.GetSelectedMessagesAsync(messagesToTweet)
                .SelectAwait(_drawableMessageFactory.CreateAsync)
                .ToListAsync();
            var image = _messageRenderer.RenderConversation(drawableMessages);
            var tweetUrl = await _imageTweetSender.TweetImageAsync(image);
            return CommandResult.FromSuccess(tweetUrl);
        }

        private async Task<RuntimeResult> GetRandomTweetAsync(string displayName, string? filter, bool onlyMedia)
        {
            var tweet = await _randomTweetProvider.GetRandomTweetAsync(displayName, onlyMedia, filter) ??
                        throw new ModuleException("no tweets that matched the filter were found");
            return CommandResult.FromSuccess($"https://twitter.com/{tweet.AuthorName}/status/{tweet.Id}");
        }
    }
}