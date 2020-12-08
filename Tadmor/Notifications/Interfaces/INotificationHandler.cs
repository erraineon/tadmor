using System.Threading;
using System.Threading.Tasks;

namespace Tadmor.Notifications.Interfaces
{
    public interface INotificationHandler<in TNotification>
    {
        Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
    }
}