using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Discord.Notifications;
using Tadmor.Core.Extensions;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Utilities.Modules;
using Tadmor.Utilities.Services;

namespace Tadmor.Utilities.Extensions
{
    public static class UtilitiesRegistrationExtensions
    {
        public static IHostBuilder UseUtilities(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddTransient<INotificationHandler<ChatClientReadyNotification>, BotStartupNotifier>();
                })
                .UseModule<UtilitiesModule>();
        }
    }
}