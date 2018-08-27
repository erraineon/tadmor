using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using Tadmor.Extensions;
using Tadmor.Services.FaceApp;

namespace Tadmor.Modules
{
    public class FaceAppModule : ModuleBase<ICommandContext>
    {
        private readonly FaceAppService _faceApp;

        public FaceAppModule(FaceAppService faceApp)
        {
            _faceApp = faceApp;
        }

        [Command("faceapp")]
        public async Task Faceapp(string filterId, string url = null)
        {
            var imageUrls = await Context.GetAllImageUrls(new[]{ url });
            var imageUrl = imageUrls.FirstOrDefault() ?? throw new Exception("need an image");
            var stream = await _faceApp.Filter(imageUrl, filterId);
            await Context.Channel.SendFileAsync(stream, "result.png");
        }

        [Command("faceapp")]
        public async Task Faceapp()
        {
            var filters = await _faceApp.GetFilters();
            await ReplyAsync(filters.Keys.Humanize());
        }
    }
}