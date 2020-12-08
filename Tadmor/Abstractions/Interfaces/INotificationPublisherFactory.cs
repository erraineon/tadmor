using Tadmor.Notifications.Interfaces;

namespace Tadmor.Abstractions.Interfaces
{
    public interface INotificationPublisherFactory
    {
        INotificationPublisher Create();
    }
}