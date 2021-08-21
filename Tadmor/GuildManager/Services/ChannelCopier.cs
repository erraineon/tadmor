using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Tadmor.GuildManager.Interfaces;

namespace Tadmor.GuildManager.Services
{
    public class ChannelCopier : IChannelCopier
    {
        public async Task<int> CopyAsync(IEnumerable<ITextChannel> sources, ITextChannel destination, bool onlyImages)
        {
            var messagesByChannel = await Task.WhenAll(sources
                .Select(source => GetAllMessagesAsync(source, onlyImages)));
            var sortedFormattedMessages = messagesByChannel
                .SelectMany(messagesInChannel => messagesInChannel)
                .OrderBy(m => m.Id)
                .Select(FormatMessage)
                .ToList();
            foreach (var messageToSend in sortedFormattedMessages)
            {
                await destination.SendMessageAsync(messageToSend);
            }

            return sortedFormattedMessages.Count;
        }

        private static string FormatMessage(IMessage message)
        {
            var attachmentUrls = string.Join(' ', message.Attachments
                .Where(a => !message.Content.Contains(a.Url))
                .Select(a => a.ProxyUrl));
            var opName = message.Author.Username;
            var opChannel = ((ITextChannel) message.Channel).Mention;
            var result = $"(OP: {opName} in {opChannel} at {message.Timestamp:g}): {message.Content} {attachmentUrls}";
            return result;
        }

        private static async Task<List<IMessage>> GetAllMessagesAsync(ITextChannel channel, bool onlyImages)
        {
            var channelMessages = new List<IMessage>();
            var fromMessageId = 0UL;
            while (await channel.GetMessagesAsync(fromMessageId, Direction.After)
                    .Flatten()
                    .Where(m => !onlyImages || m.Attachments.Any())
                    .ToListAsync()
                is {Count: > 0} batch)
            {
                fromMessageId = batch.Max(m => m.Id);
                channelMessages.AddRange(batch);
            }

            return channelMessages;
        }
    }
}