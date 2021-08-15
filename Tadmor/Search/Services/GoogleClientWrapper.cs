using Google.Apis.Customsearch.v1;
using Tadmor.Search.Interfaces;
using Tadmor.Search.Models;

namespace Tadmor.Search.Services
{
    public class GoogleClientWrapper : CustomsearchService, IGoogleClient
    {
        public GoogleClientWrapper(GoogleOptions googleOptions) : base(new Initializer {ApiKey = googleOptions.ApiKey})
        {
        }
    }
}