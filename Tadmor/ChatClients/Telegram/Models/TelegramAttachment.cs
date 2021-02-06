using Discord;
using Telegram.Bot.Types;

namespace Tadmor.ChatClients.Telegram.Models
{
    public class TelegramAttachment : IAttachment
    {
        public ulong Id => throw new System.NotImplementedException();
        public string Filename { get; init; }
        public string Url => throw new System.NotImplementedException();
        public string ProxyUrl => throw new System.NotImplementedException();
        public int Size => throw new System.NotImplementedException();
        public int? Height => throw new System.NotImplementedException();
        public int? Width => throw new System.NotImplementedException();
    }
}