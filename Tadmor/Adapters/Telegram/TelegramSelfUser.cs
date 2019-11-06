using System;
using System.Threading.Tasks;
using Discord;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Tadmor.Adapters.Telegram
{
    public class TelegramSelfUser : TelegramUser, ISelfUser
    {
        private readonly TelegramBotClient _api;

        public TelegramSelfUser(TelegramBotClient api) : base(new User())
        {
            _api = api;
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
        public override ulong Id => (ulong) _api.BotId;
    }
}