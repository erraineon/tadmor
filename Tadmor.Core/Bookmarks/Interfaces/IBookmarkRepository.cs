using System.Threading;
using System.Threading.Tasks;

namespace Tadmor.Core.Bookmarks.Interfaces
{
    public interface IBookmarkRepository
    {
        Task UpdateLastSeenAsync(string key, string lastSeenId, CancellationToken cancellationToken = default);
        Task<string?> GetLastSeenValueAsync(string key, CancellationToken cancellationToken = default);
    }
}