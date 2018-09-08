using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoreLinq;
using Tadmor.Services.Data;

namespace Tadmor.Services.Twitter
{
    public class TwitterService
    {
        private readonly TwitterOptions _options;
        private TwitterContext _context;

        public TwitterService(IOptions<TwitterOptions> options)
        {
            _options = options.Value;
        }

        public async Task<string> GetRandomMediaStatusUrl(AppDbContext context, string username)
        {
            var posts = await GetImagePosts(context, username);
            var post = posts.RandomSubset(1).SingleOrDefault() ?? throw new Exception("there are no media statuses");
            return $"https://twitter.com/statuses/{post.TweetId}";
        }

        private async Task<List<TwitterMedia>> GetImagePosts(AppDbContext context, string displayName)
        {
            var mediaQuery = context.TwitterMedia
                .Where(m => string.Equals(m.Username, displayName, StringComparison.OrdinalIgnoreCase));
            var lastCachedTweet = mediaQuery
                .Select(m => m.TweetId)
                .DefaultIfEmpty(0UL)
                .Max();
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
                        Username = tweet.ScreenName,
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
            var twitterContext = await GetContext();
            ulong maxId = default;
            var combinedSearchResults = new List<Status>();
            List<Status> tweets;
            do
            {
                var query = twitterContext.Status.Where(tweet =>
                    tweet.Type == StatusType.User &&
                    tweet.ScreenName == displayName &&
                    tweet.Count == 200 &&
                    tweet.TweetMode == TweetMode.Compat &&
                    !tweet.IncludeRetweets);
                if (maxId != default) query = query.Where(tweet => tweet.MaxID == maxId);
                if (minId != default) query = query.Where(tweet => tweet.SinceID == minId);
                tweets = await query.ToListAsync();
                combinedSearchResults.AddRange(tweets);
                if (tweets.Any()) maxId = tweets.Min(status => status.StatusID) - 1;
            } while (tweets.Any());

            return combinedSearchResults;
        }

        private async Task<TwitterContext> GetContext()
        {
            if (_context == null)
            {
                var authorizer = new ApplicationOnlyAuthorizer
                {
                    CredentialStore = new InMemoryCredentialStore
                    {
                        ConsumerKey = _options.ConsumerKey,
                        ConsumerSecret = _options.ConsumerSecret
                    }
                };

                await authorizer.AuthorizeAsync();
                _context = new TwitterContext(authorizer);
            }

            return _context;
        }
    }
}