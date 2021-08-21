using Discord;
using Discord.Commands;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandContextFactory
    {
        ICommandContext Create(
            string command,
            IGuildChannel executeIn,
            IUser executeAs,
            IChatClient chatClient,
            IUserMessage? referencedMessage);
    }
}