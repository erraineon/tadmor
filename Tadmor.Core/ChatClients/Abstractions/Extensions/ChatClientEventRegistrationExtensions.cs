using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Abstractions.Services;

namespace Tadmor.Core.ChatClients.Abstractions.Extensions
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