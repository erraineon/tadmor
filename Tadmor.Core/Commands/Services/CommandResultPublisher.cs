using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var textsToSend = SplitLongText(result).ToList();
            foreach (var textToSend in textsToSend)
            {
                switch (result)
                {
                    case CommandResult commandResult:
                        var replyToOrNull = commandResult.Reply ? replyTo : default;
                        if (commandResult.File is { } file)
                            await commandContext.Channel.SendFileAsync(
                                new MemoryStream(file.Data),
                                file.FileName,
                                textToSend,
                                messageReference: replyToOrNull);
                        else
                            await commandContext.Channel.SendMessageAsync(
                                textToSend,
                                messageReference:
                                replyToOrNull);
                        break;
                    case RuntimeResult:
                        await commandContext.Channel.SendMessageAsync(textToSend);
                        break;
                    case ExecuteResult { Exception: ModuleException e }:
                        await commandContext.Channel.SendMessageAsync(e.Message, messageReference: replyTo);
                        break;
                    case ExecuteResult { Exception: { } }:
                        await commandContext.Channel.SendMessageAsync("TADMOR IS DEAD", messageReference: replyTo);
                        break;
                }
            }
        }

        private static IEnumerable<string> SplitLongText(IResult? result)
        {
            // splits the string based off the last found line break, if any, to fit the character limit
            var textToSend = result?.ErrorReason ?? string.Empty;
            var index = 0;
            const int maxMessageLength = 2000;
            while (textToSend.Length > maxMessageLength + index)
            {
                var lastLineBreakIndex = textToSend.LastIndexOf('\n', index + maxMessageLength);
                if (lastLineBreakIndex == -1) lastLineBreakIndex = maxMessageLength;
                yield return textToSend[index..lastLineBreakIndex];
                index += lastLineBreakIndex;
            }

            yield return textToSend[index..];
        }
    }
}