using Discord;
using Telegram.Bot.Types;

namespace Tadmor.Adapters.Telegram
{
    public class TelegramAttachment : IAttachment
    {
        private readonly PhotoSize _photoSize;

        public TelegramAttachment(PhotoSize photoSize)
        {
            _photoSize = photoSize;
        }

        public ulong Id => throw new System.NotImplementedException();
        public string Filename => _photoSize.FileId;
        public string Url => throw new System.NotImplementedException();
        public string ProxyUrl => throw new System.NotImplementedException();
        public int Size => _photoSize.FileSize;
        public int? Height => _photoSize.Height;
        public int? Width => _photoSize.Width;
    }
}