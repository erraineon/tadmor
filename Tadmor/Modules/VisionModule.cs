using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using Tadmor.Extensions;
using Tadmor.Services.FaceApp;
using Tadmor.Services.Imaging;

namespace Tadmor.Modules
{
    [Summary("face manipulation")]
    public class VisionModule : ModuleBase<ICommandContext>
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly VisionService _vision;
        private readonly FaceAppService _faceApp;

        public VisionModule(VisionService vision, FaceAppService faceApp)
        {
            _vision = vision;
            _faceApp = faceApp;
        }

        [Summary("morph two faces")]
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

        [Summary("swap two faces")]
        [Command("swap")]
        public async Task Swap(params string[] urls)
        {
            var allUrls = await Context.GetAllImageUrls(urls);
            if (!allUrls.Any()) throw new Exception("need at least an image");
            var images = await Task.WhenAll(allUrls.Take(2).Select(url => Client.GetByteArrayAsync(url)));
            var result = await _vision.Swap(images);
            await Context.Channel.SendFileAsync(result, "result.jpg");
        }

        [Summary("apply a filter to a face")]
        [Command("faceapp")]
        public async Task Faceapp(string filterId, string url = null)
        {
            var imageUrls = await Context.GetAllImageUrls(new[] { url });
            var imageUrl = imageUrls.FirstOrDefault() ?? throw new Exception("need an image");
            var stream = await _faceApp.Filter(imageUrl, filterId);
            await Context.Channel.SendFileAsync(stream, "result.png");
        }

        [Summary("view available filters")]
        [Command("faceapp")]
        public async Task Faceapp()
        {
            var filters = await _faceApp.GetFilters();
            await ReplyAsync(filters.Keys.Humanize());
        }
    }
}