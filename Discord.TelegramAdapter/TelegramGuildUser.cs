using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Discord.TelegramAdapter
{
    public class TelegramGuildUser : TelegramUser, IGuildUser
    {
        private readonly TelegramClient _telegram;
        private readonly User _user;

        public TelegramGuildUser(TelegramClient telegram, TelegramGuild guild, User user) : base(user)
        {
            _telegram = telegram;
            _user = user;
            Guild = guild;
            var isAdmin = guild.AdministratorIds.Contains(Id);
            GuildPermissions = isAdmin
                ? GuildPermissions.All
                : new GuildPermissions(sendMessages: true);
            ChannelPermissions = isAdmin ? ChannelPermissions.Text : ChannelPermissions.None;
        }

        public ChannelPermissions ChannelPermissions { get; }



        public bool IsDeafened { get; }
        public bool IsMuted { get; }
        public bool IsSelfDeafened { get; }
        public bool IsSelfMuted { get; }
        public bool IsSuppressed { get; }
        public IVoiceChannel VoiceChannel { get; }
        public string VoiceSessionId { get; }

        public ChannelPermissions GetPermissions(IGuildChannel channel)
        {
            return ChannelPermissions;
        }

        public Task KickAsync(string reason = null, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddRoleAsync(IRole role, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset? JoinedAt { get; }
        public string Nickname => _user.FirstName;

        public GuildPermissions GuildPermissions { get; }
        public IGuild Guild { get; }
        public ulong GuildId => Guild.Id;
        public IReadOnlyCollection<ulong> RoleIds { get; }

        protected bool Equals(TelegramGuildUser other)
        {
            return Equals(_user.Id, other._user.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TelegramGuildUser) obj);
        }

        public override int GetHashCode()
        {
            return _user != null ? _user.Id.GetHashCode() : 0;
        }

        public async Task<(byte[] data, string avatarId)?> GetAvatarAsync()
        {
            return await _telegram.GetAvatarAsync(Id);
        }
    }
}