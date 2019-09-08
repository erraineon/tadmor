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
            IDiscordClient client;
            IMessageChannel channel;
            switch (options.ContextType)
            {
                case CommandJobContextType.Discord:
                    channel = _discordClient.GetChannel(options.ChannelId) as IMessageChannel ??
                                  throw new Exception("channel gone, delete schedule");
                    client = _discordClient;
                    break;
                case CommandJobContextType.Telegram:
                    var chatId = new ChatId((long)options.ChannelId);
                    var telegramGuild = await _telegram.GetTelegramGuild(chatId) ??
                                         throw new Exception("chat gone, delete schedule");
                    channel = telegramGuild;
                    client = _telegram.Wrapper;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var author = await channel.GetUserAsync(options.OwnerId);
            var message = new ServiceUserMessage(channel, author, options.Command);
            var context = new CommandContext(client, message);
            await _commands.ExecuteCommand(context, string.Empty);
        }
    }
}