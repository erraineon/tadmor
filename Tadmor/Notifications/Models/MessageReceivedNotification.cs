using Discord;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.Notifications.Models
{
    public record MessageReceivedNotification(IChatClient ChatClient, IMessage Message);
}