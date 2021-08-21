using System.Collections.Generic;
using Discord;

namespace Tadmor.Core.ChatClients.Telegram.Interfaces
{
    public interface IGuildUserCache
    {
        IReadOnlyCollection<IGuildUser> GetGuildUsers(ulong guildId);
    }
}