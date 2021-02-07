using System.Threading.Tasks;
using Tadmor.ChatClients.Abstractions.Interfaces;
using Telegram.Bot.Types;

namespace Tadmor.ChatClients.Telegram.Interfaces
{
    public interface ITelegramEventProvider : IChatEventProvider
    {
        Task HandleInboundMessageAsync(Message apiMessage);
    }
}