extern alias reactive; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Telegram.Bot.Types;

namespace Tadmor.Services.Telegram
{
    public class TelegramUserMessage : IUserMessage
    {
        private readonly Message _message;

        public TelegramUserMessage(TelegramGuild guild, TelegramGuildUser user,
            Message message)
        {
            _message = message;
            Channel = guild;
            Author = user;
        }

        public ulong Id => (ulong) _message.MessageId;
        public DateTimeOffset CreatedAt => _message.Date;

        public Task DeleteAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public MessageType Type => MessageType.Default;
        public MessageSource Source => _message.From.IsBot ? MessageSource.Bot : MessageSource.User;
        public bool IsTTS => false;
        public bool IsPinned { get; }
        public string Content => _message.Text ?? string.Empty;
        public DateTimeOffset Timestamp => _message.Date;
        public DateTimeOffset? EditedTimestamp => _message.EditDate;
        public IMessageChannel Channel { get; }
        public IUser Author { get; }
        public IReadOnlyCollection<IAttachment> Attachments => new IAttachment[0];
        public IReadOnlyCollection<IEmbed> Embeds => new IEmbed[0];
        public IReadOnlyCollection<ITag> Tags => new ITag[0];
        public IReadOnlyCollection<ulong> MentionedChannelIds => new ulong[0];
        public IReadOnlyCollection<ulong> MentionedRoleIds => new ulong[0];
        public IReadOnlyCollection<ulong> MentionedUserIds => new ulong[0];
        public MessageActivity Activity { get; }
        public MessageApplication Application { get; }

        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task PinAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task UnpinAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllReactionsAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public reactive::System.Collections.Generic.IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string Resolve(TagHandling userHandling = TagHandling.Name,
            TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name,
            TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
        {
            return Content;
        }

        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions { get; }
    }
}