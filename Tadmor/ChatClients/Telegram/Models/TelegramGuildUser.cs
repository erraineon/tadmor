using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Tadmor.ChatClients.Telegram.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Tadmor.ChatClients.Telegram.Models
{
    public class TelegramGuildUser : TelegramUser, ITelegramGuildUser
    {
        private readonly ChatMember _chatMember;
        private readonly ITelegramBotClient _api;

        public TelegramGuildUser(ITelegramGuild guild, ChatMember chatMember, ITelegramBotClient api) : base(chatMember.User)
        {
            _chatMember = chatMember;
            _api = api;
            Guild = guild;
        }

        public ChannelPermissions GetPermissions(IGuildChannel channel)
        {
            throw new NotImplementedException();
        }

        public Task KickAsync(string? reason = null, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddRoleAsync(IRole role, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRoleAsync(IRole role, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset? JoinedAt => throw new NotImplementedException();
        public string? Nickname => null;

        public GuildPermissions GuildPermissions => _chatMember.Status == ChatMemberStatus.Administrator
            ? GuildPermissions.All
            : new GuildPermissions(sendMessages: true);

        public IGuild Guild { get; }
        public ulong GuildId => Guild.Id;

        public DateTimeOffset? PremiumSince => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> RoleIds => throw new NotImplementedException();
        public bool? IsPending => throw new NotImplementedException();

        public bool IsDeafened => throw new NotImplementedException();

        public bool IsMuted => throw new NotImplementedException();

        public bool IsSelfDeafened => throw new NotImplementedException();

        public bool IsSelfMuted => throw new NotImplementedException();

        public bool IsSuppressed => throw new NotImplementedException();

        public IVoiceChannel VoiceChannel => throw new NotImplementedException();

        public string VoiceSessionId => throw new NotImplementedException();

        public bool IsStreaming => throw new NotImplementedException();
    }
}