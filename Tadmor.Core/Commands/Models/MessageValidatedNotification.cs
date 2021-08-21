using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.Commands.Models
{
    public record MessageValidatedNotification(
        IChatClient ChatClient,
        IUserMessage UserMessage,
        IGuildChannel GuildChannel,
        IGuildUser GuildUser);
}