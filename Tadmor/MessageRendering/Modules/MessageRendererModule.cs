using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Extensions;
using Tadmor.MessageRendering.Interfaces;

namespace Tadmor.MessageRendering.Modules
{
    [HideInHelp]
    public class MessageRendererModule : ModuleBase<ICommandContext>
    {
        private readonly IDrawableMessageFactory _drawableMessageFactory;
        private readonly IMessageRenderer _messageRenderer;

        public MessageRendererModule(
            IMessageRenderer messageRenderer,
            IDrawableMessageFactory drawableMessageFactory)
        {
            _messageRenderer = messageRenderer;
            _drawableMessageFactory = drawableMessageFactory;
        }

        [Command("render")]
        public async Task<RuntimeResult> Render(int messagesCount = 1)
        {
            var selectedMessages = await Context.GetSelectedMessagesAsync(messagesCount).ToListAsync();
            var drawableMessages = await Task.WhenAll(selectedMessages.Select(_drawableMessageFactory.CreateAsync));
            var image = _messageRenderer.RenderConversation(drawableMessages);
            return CommandResult.FromSuccess(image);
        }
    }
}