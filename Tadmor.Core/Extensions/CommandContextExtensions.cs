using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Tadmor.Core.Extensions
{
    public static class CommandContextExtensions
    {
        public static IAsyncEnumerable<IMessage> GetSelectedMessagesAsync(this ICommandContext context, int messagesCount = 100)
        {
            var maxId = context.Message.ReferencedMessage is { } repliedToMessage
                ? repliedToMessage.Id + 1
                : context.Message.Id;
            var messages = context.Channel.GetMessagesAsync(maxId, Direction.Before, messagesCount).Flatten();
            return messages;
        }

        public static async Task<IMessage> GetSelectedMessageAsync(this ICommandContext context)
        {
            var message = await GetSelectedMessagesAsync(context, 1).FirstOrDefaultAsync();
            return message;
        }
    }
}