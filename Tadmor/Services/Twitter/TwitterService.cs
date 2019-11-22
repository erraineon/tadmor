using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoreLinq;
using Tadmor.Services.Data;

namespace Tadmor.Services.Twitter
{
    [SingletonService]
    public class TwitterService
    {
        private readonly Lazy<Task<TwitterContext>> _lazyContext;
        private readonly TwitterOptions _twitterOptions;

        public TwitterService(IOptions<TwitterOptions> options)
        {
            _twitterOptions = options.Value;
            _lazyContext = new Lazy<Task<TwitterContext>>(GetContextAsync);
        }

        private async Task<TwitterContext> GetContextAsync()
        {
            var authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = _twitterOptions.ConsumerKey,
                    ConsumerSecret = _twitterOptions.ConsumerSecret,
                    OAuthToken = _twitterOptions.OAuthToken,
                    OAuthTokenSecret = _twitterOptions.OAuthTokenSecret,
                }
            };

            await authorizer.AuthorizeAsync();
            var context = new TwitterContext(authorizer);
            var verifyResponse = await context.Account
                .Where(acct => acct.Type == AccountType.VerifyCredentials)
                .SingleOrDefaultAsync();
            if (verifyResponse?.User is { } user)
            {
                context.Authorizer.CredentialStore.ScreenName = user.ScreenNameResponse;
            }
            return context;
        }

        public async Task<string> Tweet(string value)
        {
            var context = await _lazyContext.Value;
            var myName = context.Authorizer.CredentialStore.ScreenName;
            var tweet = await context.TweetAsync(value);
            return $"https://twitter.com/{myName}/status/{tweet.StatusID}";
        }

        public async Task<string> Tweet(byte[] value)
        {
            var context = await _lazyContext.Value;
            var myName = context.Authorizer.CredentialStore.ScreenName;
            var media = await context.UploadMediaAsync(value, "image/png", "tweet_image");
            var tweet = await context.TweetAsync(string.Empty, new[] {media.MediaID});
            return $"https://twitter.com/{myName}/status/{tweet.StatusID}";
        }


        public async Task<string> GetRandomMediaStatusUrl(AppDbContext context, string username)
        {
            var posts = await GetImagePosts(context, username);
            var post = posts.RandomSubset(1).SingleOrDefault() ?? throw new Exception("there are no media statuses");
            return $"https://twitter.com/{username}/status/{post.TweetId}";
        }

        private async Task<List<TwitterMedia>> GetImagePosts(AppDbContext context, string displayName)
        {
            var mediaQuery = context.TwitterMedia
                .AsQueryable()
                .Where(m => EF.Functions.Like(displayName, m.Username));
            var lastCachedTweet = (ulong) mediaQuery
                .Select(m => m.TweetId)
                .AsEnumerable()
                .DefaultIfEmpty()
                .Max(ti => (long) ti);
            var tweets = await GetTweets(displayName, lastCachedTweet);
            var newMedia = tweets
                .SelectMany(tweet => tweet.ExtendedEntities.MediaEntities
                    .Select(entity => new TwitterMedia
                    {
                        Url = entity.Type == "video"
                            ? entity.VideoInfo.Variants.MaxBy(v => v.BitRate).First().Url
                            : entity.MediaUrl,
                        StatusText = tweet.Entities.MediaEntities
                            .Aggregate(tweet.Text, (t, e) => t.Replace(e.Url, string.Empty)),
                        TweetId = tweet.StatusID,
                        MediaId = entity.ID,
                        Username = tweet.ScreenName
                    }))
                .ToList();
            if (newMedia.Any())
            {
                await context.TwitterMedia.AddRangeAsync(newMedia);
                await context.SaveChangesAsync();
            }
            return await EntityFrameworkQueryableExtensions.ToListAsync(mediaQuery);
        }

        private async Task<List<Status>> GetTweets(string displayName, ulong minId)
        {
            ulong maxId = default;
            var combinedSearchResults = new List<Status>();
            List<Status> tweets;
            var context = await _lazyContext.Value;
            do
            {
                var query = context.Status.Where(tweet =>
                    tweet.Type == StatusType.User &&
                    tweet.ScreenName == displayName &&
                    tweet.Count == 200 &&
                    tweet.TweetMode == TweetMode.Compat &&
                    tweet.IncludeRetweets == false);
                // ReSharper disable once AccessToModifiedClosure
                if (maxId != default) query = query.Where(tweet => tweet.MaxID == maxId);
                if (minId != default) query = query.Where(tweet => tweet.SinceID == minId);
                tweets = await query.ToListAsync();
                combinedSearchResults.AddRange(tweets);
                if (tweets.Any()) maxId = tweets.Min(status => status.StatusID) - 1;
            } while (tweets.Any());

            return combinedSearchResults;
        }
    }
}