using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;

namespace Tadmor.Modules
{
    public class VisionModule : ModuleBase<ICommandContext>
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly VisionService _vision;

        public VisionModule(VisionService vision)
        {
            _vision = vision;
        }

        [Command("tf")]
        public async Task Morph(string url1 = null, string url2 = null)
        {
            var sourceImageUrl = await Context.GetImageUrl(url1);
            var destImageUrl = await Context.GetImageUrl(url2);
            var sourceImage = await Client.GetByteArrayAsync(sourceImageUrl);
            var destImage = await Client.GetByteArrayAsync(destImageUrl);
            var stream = await _vision.Morph(sourceImage, destImage);
            await Context.Channel.SendFileAsync(stream, "result.gif");
        }
    }
}