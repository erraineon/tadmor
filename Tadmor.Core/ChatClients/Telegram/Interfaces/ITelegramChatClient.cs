using System.Threading.Tasks;
using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.ChatClients.Telegram.Interfaces
{
    public interface ITelegramChatClient : IChatClient
    {
        Task<byte[]> DownloadFileAsync(string fileId);
        Task<string?> GetAvatarIdAsync(IUser user);
    }
}