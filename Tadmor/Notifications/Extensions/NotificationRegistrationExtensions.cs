using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Extensions;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Services;
using Tadmor.Rules.Modules;

namespace Tadmor.Notifications.Extensions
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