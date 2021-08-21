using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Interfaces
{
    public interface ITelegramGuildUserFactory
    {
        Task<ITelegramGuildUser?> CreateOrNullAsync(ITelegramGuild telegramGuild, long userId);
        ITelegramGuildUser Create(ITelegramGuild telegramGuild, ChatMember chatMember);
        ITelegramGuildUser Create(ITelegramGuild telegramGuild, User user, bool isAdmin);
    }
}