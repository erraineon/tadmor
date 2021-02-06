using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Tadmor.ChatClients.Telegram.Interfaces
{
    public interface ITelegramApiClient : ITelegramBotClient
    {
        event Func<Message, Task> MessageReceivedAsync;
    }
}