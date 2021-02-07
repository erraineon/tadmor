using System.Threading.Tasks;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Tadmor.Core.ChatClients.Telegram.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramGuildUserFactory : ITelegramGuildUserFactory
    {
        private readonly ITelegramBotClient _api;

        public TelegramGuildUserFactory(ITelegramBotClient api)
        {
            _api = api;
        }

        public async Task<ITelegramGuildUser?> CreateOrNullAsync(ITelegramGuild telegramGuild, int userId)
        {
            try
            {
                var chatMember = await _api.GetChatMemberAsync(new ChatId((long)telegramGuild.Id), userId);
                return Create(telegramGuild, chatMember);
            }
            catch (UserNotFoundException)
            {
                return null;
            }
            catch (InvalidUserIdException)
            {
                return null;
            }
        }

        public ITelegramGuildUser Create(ITelegramGuild telegramGuild, ChatMember chatMember)
        {
            return new TelegramGuildUser(telegramGuild, chatMember, _api);
        }
    }
}