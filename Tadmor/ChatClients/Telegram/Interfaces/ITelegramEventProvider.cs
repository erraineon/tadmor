using System.Threading.Tasks;
using Tadmor.ChatClients.Interfaces;
using Telegram.Bot.Types;

namespace Tadmor.ChatClients.Telegram.Interfaces
{
    public interface ITelegramEventProvider : IChatEventProvider
    {
        Task HandleInboundMessageAsync(Message apiMessage);
    }
}