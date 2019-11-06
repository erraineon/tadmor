using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Discord.TelegramAdapter
{
    public class TelegramSelfUser : TelegramUser, ISelfUser
    {
        private readonly TelegramBotClient _api;

        public TelegramSelfUser(TelegramBotClient api) : base(new User())
        {
            _api = api;
        }

        public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public override ulong Id => (ulong) _api.BotId;
        public string Email { get; }
        public bool IsVerified { get; }
        public bool IsMfaEnabled { get; }
    }
}