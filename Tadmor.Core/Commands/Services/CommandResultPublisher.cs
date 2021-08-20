using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Core.ChatClients.Abstractions.Models;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Services
{
    public class CommandResultPublisher : ICommandResultPublisher
    {
        public async Task PublishAsync(PublishCommandResultRequest request, CancellationToken cancellationToken)
        {
            var (commandContext, result) = request;
            var replyTo = commandContext.Message is not ServiceUserMessage
                ? new MessageReference(
                    commandContext.Message.Id,
                    commandContext.Channel.Id,
                    commandContext.Guild.Id)
                : default;
            switch (result)
            {
                case CommandResult commandResult:
                    var replyToOrNull = commandResult.Reply ? replyTo : default;
                    if (commandResult.File is { } file)
                        await commandContext.Channel.SendFileAsync(
                            new MemoryStream(file.Data),
                            file.FileName,
                            commandResult.Reason,
                            messageReference: replyToOrNull);
                    else
                        await commandContext.Channel.SendMessageAsync(
                            commandResult.Reason,
                            messageReference:
                            replyToOrNull);
                    break;
                case RuntimeResult runtimeResult:
                    await commandContext.Channel.SendMessageAsync(runtimeResult.Reason);
                    break;
                case ExecuteResult {Exception: ModuleException e}:
                    await commandContext.Channel.SendMessageAsync(e.Message, messageReference: replyTo);
                    break;
                case ExecuteResult {Exception: {}}:
                    await commandContext.Channel.SendMessageAsync("TADMOR IS DEAD", messageReference: replyTo);
                    break;
            }
        }
    }
}