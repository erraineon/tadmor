using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Interfaces
{
    public interface ITelegramUserMessageFactory
    {
        ITelegramUserMessage Create(Message apiMessage, ITelegramGuild channel, ITelegramGuildUser author);
    }
}