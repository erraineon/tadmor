using Discord;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.Abstractions.Models
{
    public record GuildMemberUpdatedNotification(IChatClient ChatClient, IGuildUser OldUser, IGuildUser NewUser);
}