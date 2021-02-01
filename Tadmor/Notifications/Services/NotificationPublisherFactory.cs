using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tadmor.Notifications.Interfaces;

namespace Tadmor.Notifications.Services
{
    public class NotificationPublisherFactory : INotificationPublisherFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public NotificationPublisherFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public INotificationPublisher Create()
        {
            var scope = _serviceScopeFactory.CreateScope();
            var notificationPublisher = new NotificationPublisher(
                scope,
                scope.ServiceProvider.GetRequiredService<ILogger<NotificationPublisher>>());
            return notificationPublisher;
        }
    }
}