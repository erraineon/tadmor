using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using LinqToTwitter.Common;
using Tadmor.Twitter.Interfaces;
using Tweet = Tadmor.Twitter.Models.Tweet;

namespace Tadmor.Twitter.Services
{
    public class TweetProvider : ITweetProvider
    {
        private readonly ITwitterContextFactory _twitterContextFactory;

        public TweetProvider(ITwitterContextFactory twitterContextFactory)
        {
            _twitterContextFactory = twitterContextFactory;
        }

        public async Task<IList<Tweet>> GetTweetsAsync(string displayName, ulong? minimumStatusId)
        {
            var newTweets = await GetStatusesAsync(displayName, minimumStatusId ?? 0);
            var collection = newTweets.Select(s => new Tweet
            {
                Id = s.StatusID,
                AuthorName = s.User?.ScreenNameResponse ?? displayName,
                Status = s.Text ?? string.Empty,
                HasMedia = s.Entities?.MediaEntities?.Any() == true
            }).ToList();
            return collection;
        }

        private async Task<List<Status>> GetStatusesAsync(string displayName, ulong minId)
        {
            var twitterContext = await _twitterContextFactory.CreateAsync();
            ulong maxId = default;
            var tweets = new List<Status>();
            List<Status> buffer;
            do
            {
                var query = twitterContext.Status.Where(tweet =>
                    tweet.Type == StatusType.User &&
                    tweet.ScreenName == displayName &&
                    tweet.Count == 200 &&
                    tweet.TweetMode == TweetMode.Compat &&
                    tweet.IncludeRetweets == false &&
                    tweet.ExcludeReplies == true);
                // ReSharper disable once AccessToModifiedClosure
                if (maxId != default) query = query.Where(tweet => tweet.MaxID == maxId);
                if (minId != default) query = query.Where(tweet => tweet.SinceID == minId);
                buffer = await query.ToListAsync();
                tweets.AddRange(buffer);
                if (buffer.Any()) maxId = buffer.Min(status => status.StatusID) - 1;
            } while (buffer.Any());

            return tweets;
        }
    }
}