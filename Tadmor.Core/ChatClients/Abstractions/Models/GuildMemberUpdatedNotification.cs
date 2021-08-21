using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.ChatClients.Abstractions.Models
{
    public record GuildMemberUpdatedNotification(IChatClient ChatClient, IGuildUser OldUser, IGuildUser NewUser);
}