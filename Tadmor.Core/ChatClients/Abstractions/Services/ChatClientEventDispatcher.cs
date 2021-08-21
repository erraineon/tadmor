using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.ChatClients.Abstractions.Models;
using Tadmor.Core.Notifications.Interfaces;

namespace Tadmor.Core.ChatClients.Abstractions.Services
{
    public class ChatClientEventDispatcher : IHostedService
    {
        private readonly IEnumerable<IChatEventProvider> _chatEventProviders;
        private readonly INotificationPublisherFactory _notificationPublisherFactory;

        public ChatClientEventDispatcher(
            IEnumerable<IChatEventProvider> chatEventProviders,
            INotificationPublisherFactory notificationPublisherFactory)
        {
            _chatEventProviders = chatEventProviders;
            _notificationPublisherFactory = notificationPublisherFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var chatClient in _chatEventProviders)
            {
                chatClient.MessageReceived += OnMessageReceived;
                chatClient.GuildMemberUpdated += OnGuildMemberUpdated;
                chatClient.Log += OnLog;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var chatClient in _chatEventProviders)
            {
                chatClient.MessageReceived -= OnMessageReceived;
                chatClient.GuildMemberUpdated -= OnGuildMemberUpdated;
                chatClient.Log -= OnLog;
            }

            return Task.CompletedTask;
        }

        private async Task OnLog(IChatClient chatClient, LogMessage logMessage)
        {
            await PublishAsync(new LogNotification(chatClient, logMessage));
        }

        private async Task OnGuildMemberUpdated(
            IChatClient chatClient,
            IGuildUser oldUser,
            IGuildUser newUser)
        {
            await PublishAsync(new GuildMemberUpdatedNotification(chatClient, oldUser, newUser));
        }

        private Task OnMessageReceived(IChatClient chatClient, IMessage message)
        {
            _ = PublishAsync(new MessageReceivedNotification(chatClient, message));
            return Task.CompletedTask;
        }

        private async Task PublishAsync<TNotification>(TNotification notification)
        {
            using var notificationPublisher = _notificationPublisherFactory.Create();
            await notificationPublisher.PublishAsync(notification);
        }
    }
}