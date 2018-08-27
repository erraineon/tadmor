using System;
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
        public async Task Morph(params string[] urls)
        {
            var allUrls = await Context.GetAllImageUrls(urls);
            if (allUrls.Count < 2) throw new Exception("need at least two images");
            var (sourceImageUrl, destImageUrl) = (allUrls[0], allUrls[1]);
            var sourceImage = await Client.GetByteArrayAsync(sourceImageUrl);
            var destImage = await Client.GetByteArrayAsync(destImageUrl);
            var stream = await _vision.Morph(sourceImage, destImage);
            await Context.Channel.SendFileAsync(stream, "result.gif");
        }
    }
}