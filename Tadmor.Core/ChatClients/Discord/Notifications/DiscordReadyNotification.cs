using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.ChatClients.Discord.Notifications
{
    public record ChatClientReadyNotification(IChatClient ChatClient);
}