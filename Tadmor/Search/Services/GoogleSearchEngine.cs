using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Tadmor.Search.Interfaces;
using Tadmor.Search.Models;

namespace Tadmor.Search.Services
{
    public class GoogleSearchEngine : IGoogleSearchEngine
    {
        private readonly IGoogleClient _googleClient;
        private readonly GoogleOptions _googleOptions;

        public GoogleSearchEngine(GoogleOptions googleOptions, IGoogleClient googleClient)
        {
            _googleOptions = googleOptions;
            _googleClient = googleClient;
        }

        public Task<string?> FindWebpageLinkAsync(string query)
        {
            return FindFirst(query, false);
        }

        public Task<string?> FindImageLinkAsync(string query)
        {
            return FindFirst(query, true);
        }

        private async Task<string?> FindFirst(string query, bool image)
        {
            var listRequest = _googleClient.Cse.List();
            listRequest.Q = query;
            listRequest.Cx = _googleOptions.SearchEngineId;
            if (image) listRequest.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
            var search = await listRequest.ExecuteAsync();
            return search.Items.FirstOrDefault()?.Link;
        }
    }
}