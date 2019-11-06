using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Adapters.Telegram;

namespace Tadmor.Services.Abstractions
{
    [SingletonService]
    public class ChatService : IHostedService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly IServiceProvider _services;
        private readonly TelegramClient _telegramClient;

        public ChatService(DiscordSocketClient discordClient, TelegramClient telegramClient, IServiceProvider services)
        {
            _services = services;
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
            foreach (var listener in listeners) await listener.OnMessageReceivedAsync(client, message);
        }

        private async Task OnUserJoined(IDiscordClient client, IGuildUser user)
        {
            using var scope = _services.CreateScope();
            var listeners = scope.ServiceProvider.GetServices<IJoinListener>();
            foreach (var listener in listeners) await listener.OnUserJoinedAsync(client, user);
        }

        private Task OnDiscordUserJoined(SocketGuildUser arg) => UserJoined(_discordClient, arg);

        private Task OnTelegramMessageReceived(IUserMessage arg) => MessageReceived(_telegramClient, arg);

        private Task OnDiscordMessageReceived(SocketMessage arg) => MessageReceived(_discordClient, arg);
    }
}