using System;
using System.Linq;
using System.Threading.Tasks;
using Tadmor.Core.Extensions;
using Tadmor.Twitter.Interfaces;
using Tadmor.Twitter.Models;

namespace Tadmor.Twitter.Services
{
    public class TwitterService : ITwitterService
    {
        private readonly ITweetProvider _tweetProvider;
        private readonly ITwitterContextFactory _twitterContextFactory;

        public TwitterService(
            ITweetProvider tweetProvider,
            ITwitterContextFactory twitterContextFactory)
        {
            _tweetProvider = tweetProvider;
            _twitterContextFactory = twitterContextFactory;
        }

        public async Task<Tweet?> GetRandomTweetAsync(string displayName, bool onlyMedia, string? filter = default)
        {
            var tweets = await _tweetProvider.GetTweetsAsync(displayName);
            var tweet = tweets
                .Where(t =>
                    (!onlyMedia || t.HasMedia) &&
                    (filter == default || t.Status.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                .RandomOrDefault();
            return tweet;
        }

        public async Task<string> TweetImageAsync(byte[] image)
        {
            var context = await _twitterContextFactory.CreateAsync();
            var myName = context.Authorizer?.CredentialStore?.ScreenName ?? throw new Exception("failed to introspect current screen name");
            var media = await context.UploadMediaAsync(image, "image/png", "tweet_image") ?? throw new Exception("failed to upload media");
            var tweet = await context.TweetAsync(string.Empty, new[] { media.MediaID }) ?? throw new Exception("failed to send tweet");
            return $"https://twitter.com/{myName}/status/{tweet.StatusID}";
        }
    }
}