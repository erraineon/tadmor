using System.Threading.Tasks;

namespace Tadmor.Core.ChatClients.Telegram.Interfaces
{
    public interface ITelegramGuildFactory
    {
        Task<ITelegramGuild> CreateAsync(long guildId);
    }
}