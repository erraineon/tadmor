using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Notifications.Services;

namespace Tadmor.Core.Notifications.Extensions
{
    public static class NotificationRegistrationExtensions
    {
        public static IHostBuilder UseNotifications(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddTransient<INotificationPublisherFactory, NotificationPublisherFactory>()
                    .AddTransient(p => p.GetRequiredService<INotificationPublisherFactory>().Create()));
        }
    }
}