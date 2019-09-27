using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Tadmor.Services.Commands;
using Tadmor.Services.Hangfire;
using Tadmor.Services.Telegram;
using Tadmor.Utils;
using Telegram.Bot.Types;

namespace Tadmor.Services.Discord
{
    public class CommandJob : IHangfireJob<CommandJobOptions>
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly ChatCommandsService _commands;
        private readonly TelegramService _telegram;

        public CommandJob(DiscordSocketClient discordClient, ChatCommandsService commands, TelegramService telegram)
        {
            _discordClient = discordClient;
            _commands = commands;
            _telegram = telegram;
        }

        [CancelRecurrenceUponFailure]
        public async Task Do(CommandJobOptions options)
        {
            var client = options.ContextType switch
            {
                CommandJobContextType.Discord => (IDiscordClient) _discordClient,
                CommandJobContextType.Telegram => _telegram.Wrapper,
                _ => throw new ArgumentOutOfRangeException()
            };
            var channel = await client.GetChannelAsync(options.ChannelId) as IMessageChannel ??
                                      throw new Exception("channel gone, delete schedule");
            var author = await channel.GetUserAsync(options.OwnerId);
            var message = new ServiceUserMessage(channel, author, options.Command);
            var context = new CommandContext(client, message);
            await _commands.ExecuteCommand(context, string.Empty);
        }
    }
}