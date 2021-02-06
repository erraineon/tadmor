using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Tadmor.ChatClients.Telegram.Interfaces
{
    public interface ITelegramGuildUserFactory
    {
        Task<ITelegramGuildUser?> CreateOrNullAsync(ITelegramGuild telegramGuild, int userId);
        ITelegramGuildUser Create(ITelegramGuild telegramGuild, ChatMember chatMember);
    }
}