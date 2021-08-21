using System.Collections.Generic;
using System.Threading.Tasks;
using E621;

namespace Tadmor.Furry.Interfaces
{
    public interface IE621Client
    {
        Task<IList<E621Post>> Search(E621SearchOptions options);
    }
}