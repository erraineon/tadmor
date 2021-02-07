using Discord;
using Tadmor.ChatClients.Abstractions.Interfaces;

namespace Tadmor.ChatClients.Abstractions.Models
{
    public record LogNotification(IChatClient ChatClient, LogMessage LogMessage);
}