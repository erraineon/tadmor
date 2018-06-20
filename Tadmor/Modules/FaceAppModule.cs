using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Services.FaceApp;

namespace Tadmor.Modules
{
    public class FaceAppModule : ModuleBase<SocketCommandContext>
    {
        private readonly FaceAppService _faceApp;

        public FaceAppModule(FaceAppService faceApp)
        {
            _faceApp = faceApp;
        }

        [Command("faceapp")]
        public async Task Faceapp()
        {
            var imageUrl = Context.Message.Attachments.FirstOrDefault(a => a.Width != null)?.Url ??
                           throw new Exception("upload an image");
            var stream = await _faceApp.Filter(imageUrl);
            await Context.Channel.SendFileAsync(stream, "result.png");
        }
    }
}