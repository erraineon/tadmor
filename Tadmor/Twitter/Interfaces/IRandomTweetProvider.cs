using System.Threading.Tasks;
using Tadmor.Twitter.Models;

namespace Tadmor.Twitter.Interfaces
{
    public interface IRandomTweetProvider
    {
        Task<Tweet?> GetRandomTweetAsync(string displayName, bool onlyMedia, string? filter = default);
    }
}