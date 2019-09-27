using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tadmor.Services.Commands;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Tadmor.Services.Telegram
{
    public class TelegramService : IHostedService
    {

        private readonly ChatCommandsService _commands;
        private readonly TelegramOptions _options;

        public TelegramService(ChatCommandsService commands, IOptionsSnapshot<TelegramOptions> options)
        {
            _commands = commands;
            _options = options.Value;
        }

        public TelegramWrapper Wrapper { get; private set; }

        public TelegramBotClient Client { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_options.Token) && _options.Token != "your bot token")
            {
                var client = new TelegramBotClient(_options.Token);
                client.OnMessage += TryExecuteCommand;
                client.StartReceiving(cancellationToken: cancellationToken);
                Client = client;
                Wrapper = new TelegramWrapper(_options, client);
            }


            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Client.StopReceiving();
            return Task.CompletedTask;
        }

        private async void TryExecuteCommand(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            if (msg.Chat.Type != ChatType.Private)
            {
                var guild = await Wrapper.GetTelegramGuild(msg.Chat);
                var message = guild.ProcessInboundMessage(msg);
                await MessageReceived(message);
                const string commandPrefix = ".";
                if (!message.Author.IsBot && message.Content.StartsWith(commandPrefix))
                {
                    var context = new CommandContext(Wrapper, message);
                    await _commands.ExecuteCommand(context, commandPrefix);
                }
            }
        }

        public event Func<IMessage, Task> MessageReceived = _ => Task.CompletedTask;
    }
}