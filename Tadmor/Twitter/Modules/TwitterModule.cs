using System.Linq;
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
        public async Task<RuntimeResult> GetRandomTweetAsync(string displayName, [Remainder] string? filter = default)
        {
            return await GetRandomTweetAsync(displayName, filter, false);
        }

        [Command("twitter media")]
        [Priority(1)]
        public async Task<RuntimeResult> GetRandomMediaTweetAsync(string displayName, [Remainder] string? filter = default)
        {
            return await GetRandomTweetAsync(displayName, filter, true);
        }

        [Command("tweet")]
        [RequireWhitelist]
        public async Task<RuntimeResult> TweetAsync(int messagesToTweet = 1)
        {
            var selectedMessages = await Context.GetSelectedMessagesAsync(messagesToTweet).ToListAsync();
            var drawableMessages = await Task.WhenAll(selectedMessages.Select(_drawableMessageFactory.CreateAsync));
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