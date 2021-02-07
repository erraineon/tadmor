using System;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.ChatClients.Discord.Interfaces
{
    public interface IDiscordChatClient : IChatClient
    {
        event Func<Task> Ready;
        Task LoginAsync(TokenType tokenType, string token, bool validateToken);
    }
}