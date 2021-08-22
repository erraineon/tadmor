using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Discord.Interfaces;
using Tadmor.Core.ChatClients.Discord.Models;
using Tadmor.Core.ChatClients.Discord.Notifications;
using Tadmor.Core.Notifications.Interfaces;

namespace Tadmor.Core.ChatClients.Discord.Services
{
    public class DiscordClientLauncher : IHostedService
    {
        private readonly IDiscordChatClient _discordChatClient;
        private readonly DiscordOptions _discordOptions;
        private readonly INotificationPublisher _notificationPublisher;

        public DiscordClientLauncher(
            IDiscordChatClient discordChatClient,
            DiscordOptions discordOptions,
            INotificationPublisher notificationPublisher)
        {
            _discordChatClient = discordChatClient;
            _discordOptions = discordOptions;
            _notificationPublisher = notificationPublisher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var discordReady = new TaskCompletionSource();

            Task OnReady()
            {
                _discordChatClient.Ready -= OnReady;
                discordReady.SetResult();
                return Task.CompletedTask;
            }

            _discordChatClient.Ready += OnReady;
            await _discordChatClient.LoginAsync(TokenType.Bot, _discordOptions.Token, true);
            await _discordChatClient.StartAsync();
            await discordReady.Task;
            await _notificationPublisher.PublishAsync(
                new ChatClientReadyNotification(_discordChatClient),
                cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordChatClient.StopAsync();
        }
    }
}