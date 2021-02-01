using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.Abstractions.Models
{
    public class ServiceUserMessage : IUserMessage
    {
        public ServiceUserMessage(string content, IGuildChannel channel, IUser author, IUserMessage? referencedMessage)
        {
            Content = content;
            Channel = (IMessageChannel) channel;
            Author = author;
            ReferencedMessage = referencedMessage;
        }

        public ulong Id => throw new NotSupportedException();

        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

        public Task DeleteAsync(RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task AddReactionAsync(IEmote emote, RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task RemoveAllReactionsAsync(RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(
            IEmote emoji,
            int limit,
            RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public MessageType Type { get; } = MessageType.Default;
        public MessageSource Source { get; } = MessageSource.System;
        public bool IsTTS => throw new NotSupportedException();
        public bool IsPinned => throw new NotSupportedException();
        public bool IsSuppressed => throw new NotSupportedException();
        public bool MentionedEveryone => throw new NotSupportedException();
        public string Content { get; }
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
        public DateTimeOffset? EditedTimestamp => throw new NotSupportedException();
        public IMessageChannel Channel { get; }
        public IUser Author { get; }
        public IReadOnlyCollection<IAttachment> Attachments => throw new NotSupportedException();
        public IReadOnlyCollection<IEmbed> Embeds => throw new NotSupportedException();
        public IReadOnlyCollection<ITag> Tags => throw new NotSupportedException();
        public IReadOnlyCollection<ulong> MentionedChannelIds => throw new NotSupportedException();
        public IReadOnlyCollection<ulong> MentionedRoleIds => throw new NotSupportedException();
        public IReadOnlyCollection<ulong> MentionedUserIds => throw new NotSupportedException();
        public MessageActivity Activity => throw new NotSupportedException();
        public MessageApplication Application => throw new NotSupportedException();
        public MessageReference Reference => throw new NotSupportedException();
        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => throw new NotSupportedException();
        public MessageFlags? Flags => throw new NotSupportedException();

        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task ModifySuppressionAsync(bool suppressEmbeds, RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task PinAsync(RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task UnpinAsync(RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public Task CrosspostAsync(RequestOptions? options = null)
        {
            throw new NotSupportedException();
        }

        public string Resolve(
            TagHandling userHandling = TagHandling.Name,
            TagHandling channelHandling = TagHandling.Name,
            TagHandling roleHandling = TagHandling.Name,
            TagHandling everyoneHandling = TagHandling.Ignore,
            TagHandling emojiHandling = TagHandling.Name)
        {
            throw new NotSupportedException();
        }

        public IUserMessage? ReferencedMessage { get; }
    }
}