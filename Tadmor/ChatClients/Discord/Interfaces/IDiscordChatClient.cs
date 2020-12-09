using System;
using System.Threading.Tasks;
using Discord;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.ChatClients.Discord.Interfaces
{
    public interface IDiscordChatClient : IChatClient
    {
        event Func<Task> Ready;
        Task LoginAsync(TokenType tokenType, string token, bool validateToken);
    }
}