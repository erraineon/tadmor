using System.Collections.Generic;
using Discord;

namespace Tadmor.ChatClients.Telegram.Interfaces
{
    public interface IUserMessageCache
    {
        IEnumerable<IUserMessage> GetCachedUserMessages(ulong guildId);
        void AddUserMessage(IUserMessage userMessage, ulong guildId);
    }
}