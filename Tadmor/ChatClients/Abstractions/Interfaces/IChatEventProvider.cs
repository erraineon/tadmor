using System;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.ChatClients.Abstractions.Interfaces
{
    public interface IChatEventProvider
    {
        event Func<IChatClient, IMessage, Task> MessageReceived;
        event Func<IChatClient, IGuildUser, IGuildUser, Task> GuildMemberUpdated;
        event Func<IChatClient, LogMessage, Task> Log;
    }
}