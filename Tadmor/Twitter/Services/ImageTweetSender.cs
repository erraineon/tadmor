using System;
using System.Threading.Tasks;
using Tadmor.Twitter.Interfaces;

namespace Tadmor.Twitter.Services
{
    public class ImageTweetSender : IImageTweetSender
    {
        private readonly ITwitterContextFactory _twitterContextFactory;

        public ImageTweetSender(
            ITwitterContextFactory twitterContextFactory)
        {
            _twitterContextFactory = twitterContextFactory;
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