using Discord;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.Abstractions.Models
{
    public record MessageReceivedNotification(IChatClient ChatClient, IMessage Message);
}