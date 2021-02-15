using Discord;

namespace Tadmor.Core.ChatClients.Abstractions.Interfaces
{
    public interface IChatClient : IDiscordClient
    {
        string Name { get; }
    }
}