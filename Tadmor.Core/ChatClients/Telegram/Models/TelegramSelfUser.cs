using System;
using System.Threading.Tasks;
using Discord;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Models
{
    public class TelegramSelfUser : TelegramUser, ISelfUser
    {
        public TelegramSelfUser(int botId) : base(new User{Id = botId})
        {
        }

        public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public string Email => throw new NotImplementedException();
        public bool IsVerified => throw new NotImplementedException();
        public bool IsMfaEnabled => throw new NotImplementedException();
        public UserProperties Flags => throw new NotImplementedException();
        public PremiumType PremiumType => throw new NotImplementedException();
        public string Locale => throw new NotImplementedException();
    }
}