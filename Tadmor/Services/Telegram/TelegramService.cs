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
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Tadmor.Services.Telegram
{
    public class TelegramService : IHostedService
    {
        private static readonly Dictionary<long, TelegramGuild> TelegramGuildsByChatId =
            new Dictionary<long, TelegramGuild>();

        private readonly CommandService _commands;
        private readonly TelegramOptions _options;
        private readonly IServiceProvider _services;

        public TelegramService(IServiceProvider services,
            CommandService commands,
            IOptionsSnapshot<TelegramOptions> options)
        {
            _services = services;
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
            try
            {
                var msg = e.Message;
                if (msg.Chat.Type != ChatType.Private)
                {
                    var guild = await GetTelegramGuild(msg.Chat);
                    var message = guild.ProcessInboundMessage(msg);
                    await MessageReceived(message);
                    const string commandPrefix = ".";
                    if (!message.Author.IsBot && message.Content.StartsWith(commandPrefix))
                    {
                        var context = new CommandContext(Wrapper, message);
                        await ExecuteCommand(context, commandPrefix);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public event Func<IMessage, Task> MessageReceived = _ => Task.CompletedTask;

        public async Task<TelegramGuild> GetTelegramGuild(ChatId chatId)
        {
            var chat = await Client.GetChatAsync(chatId);
            return await GetTelegramGuild(chat);
        }

        private async Task<TelegramGuild> GetTelegramGuild(Chat chat)
        {
            var chatId = chat.Id;
            if (!TelegramGuildsByChatId.TryGetValue(chatId, out var guild))
            {
                var administrators = await Client.GetChatAdministratorsAsync(new ChatId(chatId));
                TelegramGuildsByChatId[chatId] = guild = new TelegramGuild(Wrapper, chat, Enumerable.ToHashSet(
                    administrators
                        .Select(a => (ulong) a.User.Id)
                        .Concat(new[] {(ulong) Client.BotId})));
            }
            string? x = "hello";
            return guild;
        }

        private async Task ExecuteCommand(ICommandContext context, string prefix)
        {
            var scope = _services.CreateScope();
            var result = await _commands.ExecuteAsync(context, prefix.Length, scope.ServiceProvider);

            // as of DependencyInjection v2.1 scope disposal is immediate whereas precondition check is asynchronous
            // therefore scope disposal must be made asynchronous too
            _commands.CommandExecuted += DisposeScope;

            Task DisposeScope(Optional<CommandInfo> _, ICommandContext completedContext, IResult __)
            {
                if (completedContext == context)
                {
                    scope.Dispose();
                    _commands.CommandExecuted -= DisposeScope;
                }

                return Task.CompletedTask;
            }

            if (result.Error == CommandError.UnmetPrecondition) await context.Channel.SendMessageAsync("no");
        }
    }
}