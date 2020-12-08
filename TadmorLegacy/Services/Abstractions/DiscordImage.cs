using System.Net.Http;
using System.Threading.Tasks;

namespace Tadmor.Services.Abstractions
{
    public class DiscordImage : Image
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public DiscordImage(string id) : base(id)
        {
        }

        public override Task<byte[]> GetDataAsync()
        {
            return _httpClient.GetByteArrayAsync(Id);
        }
    }
}