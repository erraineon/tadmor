using System.Threading.Tasks;
using Tadmor.Adapters.Telegram;

namespace Tadmor.Services.Abstractions
{
    public class TelegramImage : Image
    {
        private readonly TelegramClient _telegram;

        public TelegramImage(TelegramClient telegram, string fileId) : base(fileId)
        {
            _telegram = telegram;
        }

        public override Task<byte[]> GetDataAsync()
        {
            return _telegram.GetImageAsync(Id);
        }
    }
}