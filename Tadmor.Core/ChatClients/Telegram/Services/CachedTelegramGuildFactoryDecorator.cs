using System.Linq;
using System.Threading.Tasks;
using Tadmor.Core.ChatClients.Telegram.Interfaces;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class CachedTelegramGuildFactoryDecorator : ITelegramGuildFactory
    {
        private readonly ITelegramGuildFactory _telegramGuildFactory;
        private readonly ITelegramCachedChatRepository _telegramCachedChatRepository;

        public CachedTelegramGuildFactoryDecorator(
            ITelegramGuildFactory telegramGuildFactory,
            ITelegramCachedChatRepository telegramCachedChatRepository)
        {
            _telegramGuildFactory = telegramGuildFactory;
            _telegramCachedChatRepository = telegramCachedChatRepository;
        }

        public async Task<ITelegramGuild> CreateAsync(long guildId)
        {
            var guild = await _telegramCachedChatRepository.GetOrCreateAsync(
                guildId, 
                () => _telegramGuildFactory.CreateAsync(guildId));
            return guild;
        }
    }
}