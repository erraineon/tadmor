using System;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.ChatClients.Interfaces
{
    public interface IChatClient : IDiscordClient
    {
        event Func<IChatClient, IMessage, Task> MessageReceived;
        event Func<IChatClient, IGuildUser, IGuildUser, Task> GuildMemberUpdated;
        event Func<IChatClient, LogMessage, Task> Log;
    }
}