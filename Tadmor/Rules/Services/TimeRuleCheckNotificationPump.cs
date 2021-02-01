using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tadmor.Abstractions.Interfaces;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Notifications.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Services
{
    public class TimeRuleCheckNotificationPump : BackgroundService
    {
        private readonly INotificationPublisherFactory _notificationPublisherFactory;
        private readonly IEnumerable<IChatClient> _chatClients;

        public TimeRuleCheckNotificationPump(
            INotificationPublisherFactory notificationPublisherFactory, 
            IEnumerable<IChatClient> chatClients)
        {
            _notificationPublisherFactory = notificationPublisherFactory;
            _chatClients = chatClients;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var notificationPublisher = _notificationPublisherFactory.Create();
                var handleTasks = _chatClients
                    .Select(chatClient => notificationPublisher.PublishAsync(
                        new TimeRuleCheckNotification(chatClient),
                        stoppingToken));
                await Task.WhenAll(handleTasks);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}