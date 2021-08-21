using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.ChatClients.Abstractions.Models
{
    public record MessageReceivedNotification(IChatClient ChatClient, IMessage Message);
}