using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Tadmor.Services.Telegram;

namespace Tadmor.Services.Imaging
{
    public class AvatarDownloaderService
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly TelegramService _telegram;

        public AvatarDownloaderService(TelegramService telegram)
        {
            _telegram = telegram;
        }

        public async Task<(byte[] data, string avatarId)> DownloadAvatar(IGuildUser user)
        {
            if (user is SocketGuildUser)
            {
                var avatarUrl = user.GetAvatarUrl();
                if (avatarUrl != null)
                {
                    var data = await Client.GetByteArrayAsync(avatarUrl);
                    return (data, user.AvatarId);
                }
            }
            else if (user is TelegramGuildUser)
            {
                var photos = await _telegram.Client.GetUserProfilePhotosAsync((int) user.Id, 0, 1);
                var photo = photos.Photos[0].FirstOrDefault();
                if (photo != null)
                {
                    var memoryStream = new MemoryStream();
                    var file = await _telegram.Client.GetFileAsync(photo.FileId);
                    await _telegram.Client.DownloadFileAsync(file.FilePath, memoryStream);
                    return (memoryStream.ToArray(), photo.FileId);
                }
            }

            return default;
        }
    }
}