using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.ChatClients.Abstractions.Services;

namespace Tadmor.ChatClients.Abstractions.Extensions
{
    public static class ChatClientEventRegistrationExtensions
    {
        public static IHostBuilder UseChatClientEvents(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services.AddHostedService<ChatClientEventDispatcher>());
        }
    }
}