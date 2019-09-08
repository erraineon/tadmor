using System;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.Services.Telegram
{
    public class TelegramSelfUser : ISelfUser
    {
        public TelegramSelfUser(int botId)
        {
            Id = (ulong) botId;
        }

        public ulong Id { get; }
        public DateTimeOffset CreatedAt { get; }
        public string Mention { get; }
        public IActivity Activity { get; }
        public UserStatus Status { get; }

        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultAvatarUrl()
        {
            throw new NotImplementedException();
        }

        public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string AvatarId { get; }
        public string Discriminator { get; }
        public ushort DiscriminatorValue { get; }
        public bool IsBot => true;
        public bool IsWebhook { get; }
        public string Username { get; }

        public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string Email { get; }
        public bool IsVerified { get; }
        public bool IsMfaEnabled { get; }
    }
}