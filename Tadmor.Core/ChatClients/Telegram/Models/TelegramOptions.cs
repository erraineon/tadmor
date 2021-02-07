namespace Tadmor.Core.ChatClients.Telegram.Models
{
    public sealed record TelegramOptions(string Token, int BotOwnerId, bool Enabled);
}