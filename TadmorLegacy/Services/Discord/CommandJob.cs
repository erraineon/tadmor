using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Commands;
using Tadmor.Services.Hangfire;
using Tadmor.Utils;

namespace Tadmor.Services.Discord
{
    [SingletonService]
    public class CommandJob : IHangfireJob<CommandJobOptions>
    {
        private readonly ChatService _chatService;
        private readonly CommandsService _commands;

        public CommandJob(ChatService chatService, CommandsService commands)
        {
            _chatService = chatService;
            _commands = commands;
        }

        [CancelRecurrenceUponFailure]
        public async Task Do(CommandJobOptions options)
        {
            var client = _chatService.GetClient(options.ContextType);
            var channel = await client.GetChannelAsync(options.ChannelId) as IMessageChannel ??
                          throw new Exception("channel gone, delete schedule");
            var author = await channel.GetUserAsync(options.OwnerId);
            var command = options.Command ?? throw new Exception("the command to execute must be not null");
            var message = new ServiceUserMessage(channel, author, command);
            var context = new CommandContext(client, message);
            await _commands.ExecuteCommand(context, string.Empty);
        }
    }
}