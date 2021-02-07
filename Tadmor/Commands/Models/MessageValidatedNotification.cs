using Discord;
using Tadmor.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Commands.Models
{
    public record MessageValidatedNotification(
        IChatClient ChatClient,
        IUserMessage UserMessage,
        IGuildChannel GuildChannel,
        IGuildUser GuildUser);
}