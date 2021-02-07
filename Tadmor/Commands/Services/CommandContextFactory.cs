using Discord;
using Discord.Commands;
using Tadmor.ChatClients.Abstractions.Interfaces;
using Tadmor.ChatClients.Abstractions.Models;
using Tadmor.Commands.Interfaces;

namespace Tadmor.Commands.Services
{
    public class CommandContextFactory : ICommandContextFactory
    {
        public ICommandContext Create(
            string command,
            IGuildChannel executeIn,
            IUser executeAs,
            IChatClient chatClient,
            IUserMessage? referencedMessage)
        {
            var serviceUserMessage = new ServiceUserMessage(
                command,
                executeIn,
                executeAs,
                referencedMessage);
            var commandContext = new CommandContext(chatClient, serviceUserMessage);
            return commandContext;
        }
    }
}