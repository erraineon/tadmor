using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Interfaces
{
    public interface ITelegramGuildFactory
    {
        ITelegramGuild Create(Chat apiChat);
        Task<ITelegramGuild> CreateAsync(long guildId);
    }
}