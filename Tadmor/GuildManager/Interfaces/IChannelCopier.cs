using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.GuildManager.Interfaces
{
    public interface IChannelCopier
    {
        Task<int> CopyAsync(IEnumerable<ITextChannel> sources, ITextChannel destination, bool onlyImages);
    }
}