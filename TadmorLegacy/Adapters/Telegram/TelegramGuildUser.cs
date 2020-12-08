using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Telegram.Bot.Types;
using Image = Tadmor.Services.Abstractions.Image;

namespace Tadmor.Adapters.Telegram
{
    public class TelegramGuildUser : TelegramUser, IGuildUser
    {
        private readonly ChannelPermissions _channelPermissions;
        private readonly TelegramClient _telegram;
        private readonly User _user;

        public TelegramGuildUser(TelegramClient telegram, TelegramGuild guild, User user) : base(user)
        {
            _telegram = telegram;
            _user = user;
            Guild = guild;
            var isAdmin = guild.IsAdmin(this);
            GuildPermissions = isAdmin
                ? GuildPermissions.All
                : new GuildPermissions(sendMessages: true);
            _channelPermissions = isAdmin ? ChannelPermissions.Text : ChannelPermissions.None;
        }

        public ChannelPermissions GetPermissions(IGuildChannel channel)
        {
            return _channelPermissions;
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
        public string Nickname => _user.FirstName;
        public GuildPermissions GuildPermissions { get; }
        public IGuild Guild { get; }
        public ulong GuildId => Guild.Id;

        public DateTimeOffset? PremiumSince => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> RoleIds => throw new NotImplementedException();

        public bool IsDeafened => throw new NotImplementedException();

        public bool IsMuted => throw new NotImplementedException();

        public bool IsSelfDeafened => throw new NotImplementedException();

        public bool IsSelfMuted => throw new NotImplementedException();

        public bool IsSuppressed => throw new NotImplementedException();

        public IVoiceChannel VoiceChannel => throw new NotImplementedException();

        public string VoiceSessionId => throw new NotImplementedException();

        public bool IsStreaming => throw new NotImplementedException();

        public async Task<Image?> GetAvatarAsync()
        {
            return await _telegram.GetAvatarAsync(Id);
        }
    }
}