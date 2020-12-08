namespace Tadmor.Telegram
{
    public sealed record TelegramOptions(string Token, int BotOwnerId, bool Enabled);
}