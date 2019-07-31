using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.Utils
{
    public class ServiceUserMessage : IUserMessage
    {
        public ServiceUserMessage(IMessageChannel channel, IUser author, string content)
        {
            Channel = channel;
            Author = author;
            Content = content;
        }

        public ulong Id => throw new NotImplementedException();

        public DateTimeOffset CreatedAt => throw new NotImplementedException();

        public Task DeleteAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public MessageType Type => throw new NotImplementedException();

        public MessageSource Source => throw new NotImplementedException();

        public bool IsTTS => throw new NotImplementedException();

        public bool IsPinned => throw new NotImplementedException();

        public string Content { get; }

        public DateTimeOffset Timestamp => throw new NotImplementedException();

        public DateTimeOffset? EditedTimestamp => throw new NotImplementedException();

        public IMessageChannel Channel { get; }

        public IUser Author { get; }

        public IReadOnlyCollection<IAttachment> Attachments => throw new NotImplementedException();

        public IReadOnlyCollection<IEmbed> Embeds => throw new NotImplementedException();

        public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedChannelIds => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedRoleIds => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedUserIds => throw new NotImplementedException();

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

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit = 100, ulong? afterUserId = null, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name,
            TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => throw new NotImplementedException();
    }
}