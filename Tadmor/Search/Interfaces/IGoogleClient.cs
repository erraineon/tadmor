using Google.Apis.Customsearch.v1;

namespace Tadmor.Search.Interfaces
{
    public interface IGoogleClient
    {
        CseResource Cse { get; }
    }
}