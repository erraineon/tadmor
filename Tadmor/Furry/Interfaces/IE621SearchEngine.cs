using System.Collections.Generic;
using System.Threading.Tasks;
using E621;

namespace Tadmor.Furry.Interfaces
{
    public interface IE621SearchEngine
    {
        Task<E621Post?> SearchRandomAsync(string tags);
        Task<IList<E621Post>> SearchLatestAsync(string tags, long? afterId);
    }
}