using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tadmor.Notifications.Interfaces
{
    public interface INotificationPublisher : IDisposable
    {
        Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = new());
    }
}