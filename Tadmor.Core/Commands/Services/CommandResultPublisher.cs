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
            switch (commandResult)
            {
                case RuntimeResult runtimeResult:
                    var messageReference = runtimeResult is CommandResult {Reply: true}
                        ? new MessageReference(commandContext.Message.Id, commandContext.Channel.Id, commandContext.Guild.Id)
                        : default;
                    await commandContext.Channel.SendMessageAsync(runtimeResult.Reason, messageReference: messageReference);
                    break;
                case ExecuteResult {Exception: ModuleException e}:
                    await commandContext.Channel.SendMessageAsync(e.Message);
                    break;
            }
        }
    }
}