using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.MessageRendering.Interfaces
{
    public interface IImageProvider
    {
        Task<byte[]?> GetAvatarAsync(IUser user);
        Task<IList<byte[]>> GetImagesAsync(IMessage message);
    }
}