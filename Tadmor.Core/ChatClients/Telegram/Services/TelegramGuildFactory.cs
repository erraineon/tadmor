using System.Threading;
using System.Threading.Tasks;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Tadmor.Core.ChatClients.Telegram.Models;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramGuildFactory : ITelegramGuildFactory
    {
        private readonly ITelegramApiClient _api;
        private readonly ITelegramGuildUserFactory _telegramGuildUserFactory;
        private readonly IGuildUserCache _guildUserCache;
        private readonly IUserMessageCache _userMessageCache;
        private readonly ITelegramUserMessageFactory _telegramUserMessageFactory;

        public TelegramGuildFactory(
            ITelegramApiClient api,
            ITelegramGuildUserFactory telegramGuildUserFactory,
            IGuildUserCache guildUserCache,
            IUserMessageCache userMessageCache, 
            ITelegramUserMessageFactory telegramUserMessageFactory)
        {
            _api = api;
            _telegramGuildUserFactory = telegramGuildUserFactory;
            _guildUserCache = guildUserCache;
            _userMessageCache = userMessageCache;
            _telegramUserMessageFactory = telegramUserMessageFactory;
        }

        public async Task<ITelegramGuild> CreateAsync(long guildId)
        {
            var chat = await _api.GetChatAsync(new ChatId(guildId), CancellationToken.None);
            return new TelegramGuild(chat, _telegramGuildUserFactory, _guildUserCache, _userMessageCache, _telegramUserMessageFactory, _api); ;
        }
    }
}