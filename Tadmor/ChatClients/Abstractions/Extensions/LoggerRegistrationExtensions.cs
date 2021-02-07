using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.ChatClients.Abstractions.Models;
using Tadmor.ChatClients.Abstractions.Services;
using Tadmor.Notifications.Interfaces;

namespace Tadmor.ChatClients.Abstractions.Extensions
{
    public static class LoggerRegistrationExtensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddLogging(configLogging => configLogging.AddConsole())
                    .AddTransient<INotificationHandler<LogNotification>, ChatClientLogger>());
        }
    }
}