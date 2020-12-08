using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tadmor.Services.CustomSearch
{
    [ScopedService]
    public class CustomSearchService
    {
        private readonly CustomsearchService _innerService;
        private readonly CustomSearchOptions _options;

        public CustomSearchService(IOptions<CustomSearchOptions> options)
        {
            _options = options.Value;
            _innerService = new CustomsearchService(new BaseClientService.Initializer {ApiKey = _options.ApiKey});
        }

        public async Task<string?> SearchFirst(string query, bool image)
        {
            var listRequest = _innerService.Cse.List(query);
            listRequest.Cx = _options.SearchEngineId;
            if (image) listRequest.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
            var search = await listRequest.ExecuteAsync();
            return search.Items.FirstOrDefault()?.Link;
        }
    }
}