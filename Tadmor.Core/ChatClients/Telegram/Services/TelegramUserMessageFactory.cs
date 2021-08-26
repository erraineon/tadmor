using System.Collections.Generic;
using System.Linq;
using Discord;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Tadmor.Core.ChatClients.Telegram.Models;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramUserMessageFactory : ITelegramUserMessageFactory
    {
        private readonly IUserMessageCache _userMessageCache;

        public TelegramUserMessageFactory(IUserMessageCache userMessageCache)
        {
            _userMessageCache = userMessageCache;
        }

        public ITelegramUserMessage Create(Message apiMessage, ITelegramGuild channel, ITelegramGuildUser author)
        {
            IEnumerable<IAttachment> GetAttachments()
            {
                if (apiMessage.Photo is { } photos)
                    yield return CreateAttachment(photos.Last().FileId);
            }

            var referencedMessage = apiMessage.ReplyToMessage is { MessageId: var messageId }
                ? _userMessageCache.GetCachedUserMessages(channel.GuildId)
                    .FirstOrDefault(um => um.Id == (ulong)messageId)
                : null;

            var message = new TelegramUserMessage
            {
                ApiMessage = apiMessage,
                Attachments = GetAttachments().ToList(),
                Channel = channel,
                Author = author,
                ReferencedMessage = referencedMessage,
            };
            return message;
        }

        private TelegramAttachment CreateAttachment(string fileId)
        {
            var attachment = new TelegramAttachment
            {
                Filename = fileId
            };
            return attachment;
        }
    }
}