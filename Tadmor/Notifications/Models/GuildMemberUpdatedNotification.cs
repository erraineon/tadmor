using Discord;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.Notifications.Models
{
    public record GuildMemberUpdatedNotification(IChatClient ChatClient, IGuildUser OldUser, IGuildUser NewUser);
}