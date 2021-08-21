using System.Linq;
using System.Threading.Tasks;
using Discord;
using Tadmor.MessageRendering.Interfaces;
using Tadmor.MessageRendering.Models;

namespace Tadmor.MessageRendering.Services
{
    public class DrawableMessageFactory : IDrawableMessageFactory
    {
        private readonly IImageProvider _imageProvider;

        public DrawableMessageFactory(IImageProvider imageProvider)
        {
            _imageProvider = imageProvider;
        }

        public async ValueTask<DrawableMessage> CreateAsync(IMessage message)
        {
            var author = (message.Author as IGuildUser)?.Nickname ?? message.Author.Username;
            var avatar = await _imageProvider.GetAvatarAsync((IGuildUser)message.Author);
            var image = (await _imageProvider.GetImagesAsync(message)).FirstOrDefault();
            var drawableMessage = new DrawableMessage(author, message.Content, avatar, image);
            return drawableMessage;
        }
    }
}