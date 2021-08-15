using System.Threading.Tasks;
using LinqToTwitter;

namespace Tadmor.Twitter.Interfaces
{
    public interface ITwitterContextFactory
    {
        Task<TwitterContext> CreateAsync();
    }
}