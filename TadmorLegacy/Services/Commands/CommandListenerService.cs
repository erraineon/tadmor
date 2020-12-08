using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Options;
using Tadmor.Utils;

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
                !userMessage.Author.IsBot)
            {
                var context = new CommandContext(client, userMessage);
                if (_chatOptions.GetCommandsPrefix(channel.Guild) is var commandPrefix &&
                    userMessage.Content.StartsWith(commandPrefix))
                {
                    await _commands.ExecuteCommand(context, commandPrefix);
                }
                else if ((await IsReplyToBot(client, userMessage) || ContainsBotName(userMessage)) && 
                         await _commands.IsCommandAvailableAsync(context, "gen"))
                {
                    var serviceContext = new CommandContext(context.Client, new ServiceUserMessage(context.Channel, context.User, ".gen"));
                    await _commands.ExecuteCommand(serviceContext, commandPrefix);
                }
            }
        }

        private static bool ContainsBotName(IMessage userMessage)
        {
            return Regex.IsMatch(userMessage.Content, @"Tad(?:mor|dy)", RegexOptions.IgnoreCase);
        }

        private static async Task<bool> IsReplyToBot(IDiscordClient client, IMessage userMessage)
        {
            return (await userMessage.GetQuoteAsync())?.Author.Id == client.CurrentUser.Id;
        }
    }
}