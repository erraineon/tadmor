using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;

namespace Tadmor.Modules
{
    [Summary("face manipulation")]
    public class VisionModule : ModuleBase<ICommandContext>
    {
        private readonly VisionService _vision;

        public VisionModule(VisionService vision)
        {
            _vision = vision;
        }

        [Summary("morph two faces")]
        [Command("tf")]
        public async Task Morph(params string[] urls)
        {
            var allImages = await Context.GetAllImagesAsync(urls, false).Take(2).ToListAsync();
            if (allImages.Count < 2) throw new Exception("need at least two images");
            var sourceImage = await allImages[0].GetDataAsync();
            var destImage = await allImages[1].GetDataAsync();
            var stream = await _vision.Morph(sourceImage, destImage);
            await Context.Channel.SendFileAsync(stream, "result.gif");
        }

        [Summary("swap two faces")]
        [Command("swap")]
        public async Task Swap(params string[] urls)
        {
            var allImages = await Context.GetAllImagesAsync(urls, false).Take(2).ToListAsync();
            if (!allImages.Any()) throw new Exception("need at least an image");
            var images = await Task.WhenAll(allImages.Select(image => image.GetDataAsync()));
            var result = await _vision.Swap(images);
            await Context.Channel.SendFileAsync(result, "result.jpg");
        }
    }
}