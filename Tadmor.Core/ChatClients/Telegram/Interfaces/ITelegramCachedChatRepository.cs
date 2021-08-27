using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.Core.ChatClients.Telegram.Interfaces
{
    public interface ITelegramCachedChatRepository
    {
        Task<ITelegramGuild> GetOrCreateAsync(long guildId, Func<Task<ITelegramGuild>> factory);
        ICollection<long> GetAllChatIds();
    }
}