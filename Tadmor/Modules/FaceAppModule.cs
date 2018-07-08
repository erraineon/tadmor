using System.Threading.Tasks;
using Discord.Commands;
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
            var imageUrl = await Context.GetImageUrl(url);
            var stream = await _faceApp.Filter(imageUrl, filterId);
            await Context.Channel.SendFileAsync(stream, "result.png");
        }
    }
}