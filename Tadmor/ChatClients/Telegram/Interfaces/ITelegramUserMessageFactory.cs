using Telegram.Bot.Types;

namespace Tadmor.ChatClients.Telegram.Interfaces
{
    public interface ITelegramUserMessageFactory
    {
        ITelegramUserMessage Create(Message apiMessage, ITelegramGuild channel, ITelegramGuildUser author);
    }
}