using System.Threading;
using System.Threading.Tasks;
using Tadmor.ChatClients.Telegram.Interfaces;
using Tadmor.ChatClients.Telegram.Models;
using Telegram.Bot.Types;

namespace Tadmor.ChatClients.Telegram.Services
{
    public class TelegramGuildFactory : ITelegramGuildFactory
    {
        private readonly ITelegramApiClient _api;
        private readonly ITelegramGuildUserFactory _telegramGuildUserFactory;
        private readonly IGuildUserCache _guildUserCache;
        private readonly IUserMessageCache _userMessageCache;

        public TelegramGuildFactory(
            ITelegramApiClient api,
            ITelegramGuildUserFactory telegramGuildUserFactory,
            IGuildUserCache guildUserCache,
            IUserMessageCache userMessageCache)
        {
            _api = api;
            _telegramGuildUserFactory = telegramGuildUserFactory;
            _guildUserCache = guildUserCache;
            _userMessageCache = userMessageCache;
        }

        public ITelegramGuild Create(Chat apiChat)
        {
            return new TelegramGuild(apiChat, _telegramGuildUserFactory, _guildUserCache, _userMessageCache, _api);
        }

        public async Task<ITelegramGuild> CreateAsync(long guildId)
        {
            var chat = await _api.GetChatAsync(new ChatId(guildId), CancellationToken.None);
            return Create(chat);
        }
    }
}