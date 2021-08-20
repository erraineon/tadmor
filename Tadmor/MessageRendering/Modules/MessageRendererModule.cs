using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Core.Commands.Models;
using Tadmor.MessageRendering.Interfaces;
using Tadmor.MessageRendering.Models;

namespace Tadmor.MessageRendering.Modules
{
    public class MessageRendererModule : ModuleBase<ICommandContext>
    {
        private readonly IImageProvider _imageProvider;
        private readonly IMessageRenderer _messageRenderer;

        public MessageRendererModule(IImageProvider imageProvider, IMessageRenderer messageRenderer)
        {
            _imageProvider = imageProvider;
            _messageRenderer = messageRenderer;
        }

        [Command("render")]
        public async Task<RuntimeResult> Render(int messagesCount)
        {
            var selectedMessages = await GetSelectedMessagesAsync(messagesCount).ToListAsync();
            var drawableMessages = await Task.WhenAll(selectedMessages
                .OfType<IUserMessage>()
                .Select(async message => new DrawableMessage
                (
                    (message.Author as IGuildUser)?.Nickname ?? message.Author.Username,
                    message.Content,
                    await _imageProvider.GetAvatarAsync((IGuildUser)message.Author),
                    (await _imageProvider.GetImagesAsync(message)).FirstOrDefault()
                )));
            var image = _messageRenderer.DrawConversation(drawableMessages);
            return CommandResult.FromSuccess(image);
        }

        private IAsyncEnumerable<IMessage> GetSelectedMessagesAsync(int messagesCount = 100)
        {
            var maxId = Context.Message.ReferencedMessage is { } repliedToMessage
                ? repliedToMessage.Id + 1
                : Context.Message.Id;
            var messages = Context.Channel.GetMessagesAsync(maxId, Direction.Before, messagesCount).Flatten();
            return messages;
        }
    }
}