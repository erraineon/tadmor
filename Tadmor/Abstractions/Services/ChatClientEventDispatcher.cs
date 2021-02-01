using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Hosting;
using Tadmor.Abstractions.Models;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Notifications.Interfaces;

namespace Tadmor.Abstractions.Services
{
    public class ChatClientEventDispatcher : IHostedService
    {
        private readonly IEnumerable<IChatClient> _chatClients;
        private readonly INotificationPublisherFactory _notificationPublisherFactory;

        public ChatClientEventDispatcher(
            IEnumerable<IChatClient> chatClients,
            INotificationPublisherFactory notificationPublisherFactory)
        {
            _chatClients = chatClients;
            _notificationPublisherFactory = notificationPublisherFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var chatClient in _chatClients)
            {
                chatClient.MessageReceived += OnMessageReceived;
                chatClient.GuildMemberUpdated += OnGuildMemberUpdated;
                chatClient.Log += OnLog;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var chatClient in _chatClients)
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