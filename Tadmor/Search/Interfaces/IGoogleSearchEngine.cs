using System.Threading.Tasks;

namespace Tadmor.Search.Interfaces
{
    public interface IGoogleSearchEngine
    {
        Task<string?> FindWebpageLinkAsync(string query);
        Task<string?> FindImageLinkAsync(string query);
    }
}