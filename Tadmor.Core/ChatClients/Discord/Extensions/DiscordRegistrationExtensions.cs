using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.ChatClients.Discord.Interfaces;
using Tadmor.Core.ChatClients.Discord.Models;
using Tadmor.Core.ChatClients.Discord.Services;
using Tadmor.Core.Extensions;

namespace Tadmor.Core.ChatClients.Discord.Extensions
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