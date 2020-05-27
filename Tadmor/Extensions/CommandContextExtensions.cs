using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Utils;

namespace Tadmor.Extensions
{
    public static class CommandContextExtensions
    {
        public static async Task<IMessage> GetQuotedMessageAsync(this ICommandContext commandContext)
        {
            var quotedMessage = await ((IReplyMessage)commandContext.Message).GetQuotedMessageAsync();
            return quotedMessage;
        } 

        public static async Task<string> GetQuotedContentAsync(this ICommandContext commandContext)
        {
            var quotedMessage = await commandContext.GetQuotedMessageAsync();
            return quotedMessage.Content;
        } 
    }
}