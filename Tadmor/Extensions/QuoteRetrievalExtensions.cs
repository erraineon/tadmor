using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Tadmor.Adapters.Telegram;

namespace Tadmor.Extensions
{
    public static class QuoteRetrievalExtensions
    {
        public static async Task<bool> IsReplyAsync(this IMessage message)
        {
            return await message.GetQuoteAsync() != null;
        }

        public static async Task<IMessage?> GetQuoteAsync(this IMessage message)
        {
            // can't currently reliably obtain a quoted message on discord
            var quote = message switch
            {
                TelegramUserMessage telegramMessage => await telegramMessage.GetQuotedMessageAsync(),
                SocketMessage discordMessage => default,
                RestMessage discordMessage => default,
                _ => throw new NotSupportedException($"{message.GetType()} quote retrieval is not supported")
            };
            return quote;
        }

        private static async Task<IMessage?> GetDiscordQuotedMessageAsync(IMessage discordMessage)
        {
            var matches = Regex.Matches(discordMessage.Content, @"(?<!^>\s).+");
            if (matches.Any())
            {
                var toSearch = string.Join(Environment.NewLine, matches.Select(m => m.Value));
                var messages = await discordMessage.Channel
                    .GetMessagesAsync(discordMessage, Direction.Before, mode: CacheMode.CacheOnly)
                    .FlattenAsync();
                var quotedMessage = messages.FirstOrDefault(m => m.Content == toSearch);
                return quotedMessage;
            }

            return null;
        }
    }
}