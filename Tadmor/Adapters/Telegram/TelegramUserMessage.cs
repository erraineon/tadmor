using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Telegram.Bot.Types;

namespace Tadmor.Adapters.Telegram
{
    public class TelegramUserMessage : IUserMessage
    {
        private readonly Message _apiMessage;

        public TelegramUserMessage(TelegramGuild guild, TelegramGuildUser user,
            Message apiMessage)
        {
            IEnumerable<IAttachment> GetAttachments()
            {
                if (apiMessage.Photo is { } photos)
                    yield return new TelegramAttachment(photos.Last());
                if (apiMessage.Sticker is { } sticker)
                    yield return new TelegramAttachment(new PhotoSize
                    {
                        FileId = sticker.FileId, 
                        FileSize = sticker.FileSize, 
                        Width = sticker.Width,
                        Height = sticker.Height
                    });
            }

            _apiMessage = apiMessage;
            Channel = guild;
            Author = user;
            Attachments = GetAttachments().ToList();
        }

        public ulong Id => (ulong) _apiMessage.MessageId;
        public DateTimeOffset CreatedAt => _apiMessage.Date;

        public Task DeleteAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public MessageType Type => MessageType.Default;
        public MessageSource Source => _apiMessage.From.IsBot ? MessageSource.Bot : MessageSource.User;
        public bool IsTTS => false;
        public bool IsPinned => throw new NotImplementedException();
        public bool IsSuppressed => throw new NotImplementedException();
        public string Content => _apiMessage.Text ?? _apiMessage.Caption ?? string.Empty;
        public DateTimeOffset Timestamp => _apiMessage.Date;
        public DateTimeOffset? EditedTimestamp => _apiMessage.EditDate;
        public IMessageChannel Channel { get; }
        public IUser Author { get; }
        public IReadOnlyCollection<IAttachment> Attachments { get; }
        public IReadOnlyCollection<IEmbed> Embeds => new IEmbed[0];
        public IReadOnlyCollection<ITag> Tags => new ITag[0];
        public IReadOnlyCollection<ulong> MentionedChannelIds => new ulong[0];
        public IReadOnlyCollection<ulong> MentionedRoleIds => new ulong[0];
        public IReadOnlyCollection<ulong> MentionedUserIds => new ulong[0];
        public MessageActivity Activity => throw new NotImplementedException();
        public MessageApplication Application => throw new NotImplementedException();
        public MessageReference Reference => throw new NotImplementedException();
        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => throw new NotImplementedException();

        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task ModifySuppressionAsync(bool suppressEmbeds, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task PinAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task UnpinAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddReactionAsync(IEmote emote, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllReactionsAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(
            IEmote emoji, int limit,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public string Resolve(TagHandling userHandling = TagHandling.Name,
            TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name,
            TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
        {
            return Content;
        }

    }
}