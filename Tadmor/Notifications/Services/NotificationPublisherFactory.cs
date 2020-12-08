using Microsoft.Extensions.DependencyInjection;
using Tadmor.Abstractions.Interfaces;
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
            var notificationPublisher = scope.ServiceProvider.GetRequiredService<INotificationPublisher>();
            return notificationPublisher;
        }
    }
}