using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.Core.ChatClients.Abstractions.Models;
using Tadmor.Core.ChatClients.Abstractions.Services;
using Tadmor.Core.Notifications.Interfaces;

namespace Tadmor.Core.ChatClients.Abstractions.Extensions
{
    public static class LoggerRegistrationExtensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddLogging(configLogging => configLogging
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddConsole())
                    .AddTransient<INotificationHandler<LogNotification>, ChatClientLogger>());
        }
    }
}