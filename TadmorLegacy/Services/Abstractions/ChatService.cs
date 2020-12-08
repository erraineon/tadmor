using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.Adapters.Telegram;

namespace Tadmor.Services.Abstractions
{
    [SingletonService]
    public class ChatService : IHostedService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly IServiceProvider _services;
        private readonly ILogger<ChatService> _logger;
        private readonly TelegramClient _telegramClient;

        public ChatService(DiscordSocketClient discordClient, TelegramClient telegramClient, IServiceProvider services, ILogger<ChatService> logger)
        {
            _services = services;
            _logger = logger;
            _discordClient = discordClient;
            _telegramClient = telegramClient;
            MessageReceived += OnMessageReceived;
            UserJoined += OnUserJoined;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _discordClient.UserJoined += OnDiscordUserJoined;
            _discordClient.MessageReceived += OnDiscordMessageReceived;
            _telegramClient.MessageReceived += OnTelegramMessageReceived;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discordClient.MessageReceived -= OnDiscordMessageReceived;
            _telegramClient.MessageReceived -= OnTelegramMessageReceived;
            return Task.CompletedTask;
        }

        public IDiscordClient GetClient(ChatClientType clientType)
        {
            var client = clientType switch
            {
                ChatClientType.Discord => (IDiscordClient)_discordClient,
                ChatClientType.Telegram => _telegramClient,
                _ => throw new ArgumentOutOfRangeException(nameof(clientType))
            };
            return client;
        }

        public ChatClientType GetClientType(IDiscordClient client)
        {
            var clientType = client switch
            {
                DiscordSocketClient _ => ChatClientType.Discord,
                TelegramClient _ => ChatClientType.Telegram,
                _ => throw new ArgumentOutOfRangeException(nameof(client))
            };
            return clientType;
        }

        public event Func<IDiscordClient, IMessage, Task> MessageReceived = (client, message) => Task.CompletedTask;
        public event Func<IDiscordClient, IGuildUser, Task> UserJoined = (client, user) => Task.CompletedTask;

        private async Task OnMessageReceived(IDiscordClient client, IMessage message)
        {
            using var scope = _services.CreateScope();
            var listeners = scope.ServiceProvider.GetServices<IMessageListener>();
            foreach (var listener in listeners)
            {
                try
                {
                    await listener.OnMessageReceivedAsync(client, message);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }

        private async Task OnUserJoined(IDiscordClient client, IGuildUser user)
        {
            using var scope = _services.CreateScope();
            var listeners = scope.ServiceProvider.GetServices<IJoinListener>();
            foreach (var listener in listeners)
            {
                try
                {
                    await listener.OnUserJoinedAsync(client, user);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }

        public Task<IUserMessage> Next(Func<IUserMessage, bool> predicate, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<IUserMessage>();

            Task OnMessageReceivedNext(IDiscordClient _, IMessage msg)
            {
                if (msg is IUserMessage userMessage && (predicate == null || predicate(userMessage)))
                    tcs.TrySetResult(userMessage);
                return Task.CompletedTask;
            }

            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            MessageReceived += OnMessageReceivedNext;
            tcs.Task.ContinueWith(_ => MessageReceived -= OnMessageReceivedNext,
                TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        private Task OnDiscordUserJoined(SocketGuildUser arg) => UserJoined(_discordClient, arg);

        private Task OnTelegramMessageReceived(IUserMessage arg) => MessageReceived(_telegramClient, arg);

        private Task OnDiscordMessageReceived(SocketMessage arg) => MessageReceived(_discordClient, arg);
    }
}