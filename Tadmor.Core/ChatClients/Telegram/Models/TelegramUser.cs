using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Models
{
    public class TelegramUser : ITelegramUser
    {
        private readonly User _user;

        public TelegramUser(User user)
        {
            _user = user;
        }

        public virtual ulong Id => (ulong) _user.Id;
        public string Mention => $@"{Username}";
        public IActivity Activity => throw new NotImplementedException();
        public UserStatus Status => UserStatus.Online;
        public IImmutableSet<ClientType> ActiveClients => throw new NotImplementedException();
        public IImmutableList<IActivity> Activities => throw new NotImplementedException();

        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultAvatarUrl()
        {
            throw new NotImplementedException();
        }

        public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public string AvatarId => throw new NotSupportedException();
        public string Discriminator => string.Empty;
        public ushort DiscriminatorValue => default;
        public bool IsBot => _user.IsBot;
        public bool IsWebhook => false;
        public string Username => _user.Username;
        public UserProperties? PublicFlags => throw new NotImplementedException();
        public DateTimeOffset CreatedAt => throw new NotImplementedException();
    }
}