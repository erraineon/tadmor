using Discord;
using Tadmor.ChatClients.Abstractions.Interfaces;

namespace Tadmor.ChatClients.Abstractions.Models
{
    public record GuildMemberUpdatedNotification(IChatClient ChatClient, IGuildUser OldUser, IGuildUser NewUser);
}