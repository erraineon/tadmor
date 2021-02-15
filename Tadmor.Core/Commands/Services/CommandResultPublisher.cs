using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Services
{
    public class CommandResultPublisher : ICommandResultPublisher
    {
        public async Task PublishAsync(PublishCommandResultRequest request, CancellationToken cancellationToken)
        {
            var (commandContext, commandResult) = request;
            var replyTo = new MessageReference(
                commandContext.Message.Id, 
                commandContext.Channel.Id,
                commandContext.Guild.Id);
            switch (commandResult)
            {
                case RuntimeResult runtimeResult:
                    var replyToOrNull = runtimeResult is CommandResult {Reply: true}
                        ? replyTo
                        : default;
                    await commandContext.Channel.SendMessageAsync(runtimeResult.Reason, messageReference: replyToOrNull);
                    break;
                case ExecuteResult {Exception: ModuleException e}:
                    await commandContext.Channel.SendMessageAsync(e.Message, messageReference: replyTo);
                    break;
            }
        }
    }
}