using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.ChatClients.Abstractions.Interfaces;
using Tadmor.ChatClients.Discord.Interfaces;
using Tadmor.ChatClients.Discord.Models;
using Tadmor.ChatClients.Discord.Services;
using Tadmor.Extensions;

namespace Tadmor.ChatClients.Discord.Extensions
{
    public static class DiscordRegistrationExtensions
    {
        public static IHostBuilder UseDiscord(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(
                    (hostingContext, services) =>
                    {
                        var discordOptions =
                            services.BindConfigurationSection<DiscordOptions>(hostingContext.Configuration);
                        if (discordOptions.Enabled)
                        {
                            services.AddSingleton<DiscordClient>();
                            services.AddSingleton<IChatClient>(s => s.GetRequiredService<DiscordClient>());
                            services.AddSingleton<IDiscordChatClient>(s => s.GetRequiredService<DiscordClient>());
                            services.AddSingleton<IChatEventProvider>(s => s.GetRequiredService<DiscordClient>());
                            services.AddHostedService<DiscordClientLauncher>();
                        }
                    });
        }
    }
}