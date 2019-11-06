using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Options;

namespace Tadmor.Services.Commands
{
    [ScopedService]
    public class CommandListenerService : IMessageListener
    {
        private readonly CommandsService _commands;
        private readonly ChatOptionsService _chatOptions;

        public CommandListenerService(CommandsService commands, ChatOptionsService chatOptions)
        {
            _commands = commands;
            _chatOptions = chatOptions;
        }

        public async Task OnMessageReceivedAsync(IDiscordClient client, IMessage message)
        {
            if (message.Channel is IGuildChannel channel &&
                message is IUserMessage userMessage &&
                !userMessage.Author.IsBot &&
                _chatOptions.GetCommandsPrefix(channel.Guild) is var commandPrefix &&
                userMessage.Content.StartsWith(commandPrefix))
            {
                var context = new CommandContext(client, userMessage);
                await _commands.ExecuteCommand(context, commandPrefix);
            }
        }
    }
}