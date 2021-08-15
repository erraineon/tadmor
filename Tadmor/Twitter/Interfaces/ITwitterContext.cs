using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToTwitter;
using LinqToTwitter.Common;
using LinqToTwitter.Provider;

namespace Tadmor.Twitter.Interfaces
{
    public interface ITwitterContext
    {
        Task<Status?> TweetAsync(string status, IEnumerable<ulong> mediaIds, TweetMode tweetMode);
        TwitterQueryable<Status> Status { get; }
    }
}