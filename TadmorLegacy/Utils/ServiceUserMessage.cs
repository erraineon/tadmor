﻿using System;
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

        public ulong Id => 0;

        public DateTimeOffset CreatedAt => throw new NotImplementedException();

        public Task DeleteAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public MessageType Type => throw new NotImplementedException();

        public MessageSource Source => throw new NotImplementedException();

        public bool IsTTS => throw new NotImplementedException();

        public bool IsPinned => throw new NotImplementedException();
        public bool IsSuppressed => throw new NotImplementedException();

        public string Content { get; }

        public DateTimeOffset Timestamp => throw new NotImplementedException();

        public DateTimeOffset? EditedTimestamp => throw new NotImplementedException();

        public IMessageChannel Channel { get; }

        public IUser Author { get; }

        public IReadOnlyCollection<IAttachment> Attachments => new IAttachment[0];

        public IReadOnlyCollection<IEmbed> Embeds => new IEmbed[0];

        public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedChannelIds => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedRoleIds => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedUserIds => throw new NotImplementedException();
        public MessageActivity Activity => throw new NotImplementedException();
        public MessageApplication Application => throw new NotImplementedException();
        public MessageReference Reference => throw new NotImplementedException();

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

        public Task CrosspostAsync(RequestOptions options = null)
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

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit = 100, ulong? afterUserId = null, RequestOptions? options = null)
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