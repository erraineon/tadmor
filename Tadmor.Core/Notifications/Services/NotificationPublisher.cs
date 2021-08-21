using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tadmor.Core.Notifications.Interfaces;

namespace Tadmor.Core.Notifications.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly ILogger<NotificationPublisher> _logger;
        private readonly IServiceScope _serviceScope;

        public NotificationPublisher(IServiceScope serviceScope, ILogger<NotificationPublisher> logger)
        {
            _serviceScope = serviceScope;
            _logger = logger;
        }

        public async Task PublishAsync<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = new())
        {
            var notificationHandlers = _serviceScope.ServiceProvider.GetServices<INotificationHandler<TNotification>>();
            foreach (var notificationHandler in notificationHandlers)
                try
                {
                    await notificationHandler.HandleAsync(notification, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, $"unhandled exception from {notificationHandler.GetType()}");
                }
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}