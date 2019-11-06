using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Discord.TelegramAdapter
{
    public class TelegramUser : IUser
    {
        private readonly User _user;

        public TelegramUser(User user)
        {
            _user = user;
        }

        public virtual ulong Id => (ulong) _user.Id;
        public DateTimeOffset CreatedAt { get; }
        public string Mention => Username;
        public IActivity Activity { get; }
        public UserStatus Status => UserStatus.Online;
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return default;
        }

        public string GetDefaultAvatarUrl()
        {
            throw new NotImplementedException();
        }

        public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string AvatarId => throw new NotSupportedException();
        public string Discriminator => string.Empty;
        public ushort DiscriminatorValue => default;
        public bool IsBot => _user.IsBot;
        public bool IsWebhook => false;
        public string Username => $"@{_user.Username}";
    }
}