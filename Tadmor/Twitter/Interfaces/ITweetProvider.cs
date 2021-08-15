using System.Collections.Generic;
using System.Threading.Tasks;
using Tadmor.Twitter.Models;

namespace Tadmor.Twitter.Interfaces
{
    public interface ITweetProvider
    {
        Task<IList<Tweet>> GetTweetsAsync(string displayName, ulong? minimumStatusId = default);
    }
}