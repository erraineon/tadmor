using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tadmor.Core.Extensions;
using Tadmor.Twitter.Interfaces;
using Tadmor.Twitter.Models;

namespace Tadmor.Twitter.Services
{
    public class RandomTweetProvider : IRandomTweetProvider
    {
        private readonly ITweetProvider _tweetProvider;

        public RandomTweetProvider(
            ITweetProvider tweetProvider)
        {
            _tweetProvider = tweetProvider;
        }

        public async Task<Tweet?> GetRandomTweetAsync(string displayName, bool onlyMedia, string? filter = default)
        {
            var tweets = await _tweetProvider.GetTweetsAsync(displayName);
            var regex = filter != null
                ? new Regex(filter, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100))
                : null;
            var tweet = tweets
                .Where(t => (!onlyMedia || t.HasMedia) && regex?.IsMatch(t.Status) != false)
                .RandomOrDefault();
            return tweet;
        }
    }
}