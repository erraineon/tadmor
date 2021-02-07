using Discord;
using Tadmor.ChatClients.Abstractions.Interfaces;

namespace Tadmor.ChatClients.Abstractions.Models
{
    public record MessageReceivedNotification(IChatClient ChatClient, IMessage Message);
}