using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using E621;
using Humanizer;
using Microsoft.Net.Http.Headers;
using MoreLinq;
using Newtonsoft.Json.Linq;

namespace Tadmor.Services.FaceApp
{
    public class FaceAppService
    {
        private const string ApiUrl = "https://node-01.faceapp.io/api/v2.8/photos";

        private static readonly HttpClient Client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                {HeaderNames.UserAgent, "FaceApp/2.0.957 (Linux; Android 4.4)"},
                {"X-FaceApp-DeviceID", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8)}
            }
        };

        private readonly RateLimiter _rateLimiter = new RateLimiter(50, TimeSpan.FromMinutes(10));
        private Dictionary<string, bool> _filters;

        public async Task<Stream> Filter(string imageUrl, string filterId)
        {
            var filters = await GetFilters();
            return filters.TryGetValue(filterId, out var cropped)
                ? await Filter(imageUrl, filterId, cropped)
                : throw new Exception($"available filters: {filters.Keys.Humanize()}");
        }

        private async Task<Stream> Filter(string imageUrl, string filter, bool cropped)
        {
            var response = await UploadImage(imageUrl);
            var code = response["code"].ToString();
            await _rateLimiter.WaitToProceed();
            var filterResponse = await Client.GetAsync($"{ApiUrl}/{code}/filters/{filter}?cropped={cropped}");
            HandleErrors(filterResponse);
            return await filterResponse.Content.ReadAsStreamAsync();
        }

        public async Task<Dictionary<string, bool>> GetFilters()
        {
            if (_filters == null)
            {
                var sampleResponse = await UploadImage("https://i.imgur.com/nVsxMNp.jpg");
                var filters = sampleResponse["filters"]
                    .Select(o => (id: (string)o["id"], crop: (bool)o["is_paid"] || (bool)o["only_cropped"]))
                    .Where(t => t.id != "no-filter")
                    .ToDictionary(t => t.id, t => t.crop);
                _filters = filters;
            }
            
            return _filters;
        }

        private async Task<JObject> UploadImage(string imageUrl)
        {
            var imageStream = await Client.GetStreamAsync(imageUrl);
            var streamContent = new StreamContent(imageStream);
            var mutipartContent = new MultipartFormDataContent {{streamContent, "file", "file"}};
            await _rateLimiter.WaitToProceed();
            var response = await Client.PostAsync(ApiUrl, mutipartContent);
            HandleErrors(response);
            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            return jObject;
        }

        private static void HandleErrors(HttpResponseMessage r)
        {
            if (!r.IsSuccessStatusCode && r.Headers.TryGetValues("X-FaceApp-ErrorCode", out var errors))
                throw new Exception(errors.Humanize());
        }
    }
}