using Discord;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.Abstractions.Models
{
    public record LogNotification(IChatClient ChatClient, LogMessage LogMessage);
}