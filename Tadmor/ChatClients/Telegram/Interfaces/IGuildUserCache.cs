using System.Collections.Generic;
using Discord;

namespace Tadmor.ChatClients.Telegram.Interfaces
{
    public interface IGuildUserCache
    {
        IReadOnlyCollection<IGuildUser> GetGuildUsers(ulong guildId);
    }
}