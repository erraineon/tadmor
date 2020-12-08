namespace Tadmor.Services.Telegram
{
    [Options]
    public class TelegramOptions
    {
        public string? Token { get; set; }
        public int BotOwnerId { get; set; }
    }
}