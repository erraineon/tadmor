using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Notifications.Interfaces;

namespace Tadmor.Notifications.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly IServiceScope _serviceScope;

        public NotificationPublisher(IServiceScope serviceScope)
        {
            _serviceScope = serviceScope;
        }

        public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = new())
        {
            var notificationHandlers = _serviceScope.ServiceProvider.GetServices<INotificationHandler<TNotification>>();
            foreach (var notificationHandler in notificationHandlers)
            {
                await notificationHandler.HandleAsync(notification, cancellationToken);
            }
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}