using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.ChatClients.Abstractions.Models
{
    public record LogNotification(IChatClient ChatClient, LogMessage LogMessage);
}