using System.Threading.Tasks;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Interfaces
{
    public interface ITelegramEventProvider : IChatEventProvider
    {
        Task HandleInboundMessageAsync(Message apiMessage);
    }
}