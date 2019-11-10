using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.Adapters.Telegram;
using Tadmor.Extensions;
using Tadmor.Services;
using Tadmor.Services.Data;

namespace Tadmor
{
    public static class Program
    {
        private static async Task Main()
        {
            var host = ConfigureHost();
            await host.RunAsync();
        }

        public static IHost ConfigureHost()
        {
            const string settingsFilename = "appsettings.json";
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configApp => configApp
                    .AddJsonFile(settingsFilename, false, true)
                    .AddJsonFile("sonagen.json"))
                .ConfigureServices((hostContext, services) => services
                    .AddWritableOptions(hostContext.Configuration, settingsFilename)
                    .AddLogging()
                    .AddMemoryCache()
                    .AddDbContext<AppDbContext>(builder => builder
                        .UseSqlite(hostContext.Configuration.GetConnectionString("Main")))
                    .AddSingleton(new DiscordSocketConfig { MessageCacheSize = 100 })
                    .AddSingleton(new TelegramClientConfig { MessageCacheSize = 100 })
                    .AddSingleton<DiscordSocketClient>()
                    .AddSingleton<TelegramClient>()
                    .Scan(scan => scan
                        .FromEntryAssembly()
                        .AddClasses(classes => classes.WithAttribute<TransientServiceAttribute>())
                        .AsSelfWithInterfaces()
                        .WithTransientLifetime()
                        .AddClasses(classes => classes.WithAttribute<ScopedServiceAttribute>())
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                        .AddClasses(classes => classes.WithAttribute<SingletonServiceAttribute>())
                        .AsSelfWithInterfaces()
                        .WithSingletonLifetime())
                )
                .ConfigureLogging(configLogging => configLogging.AddConsole())
                .UseConsoleLifetime()
                .Build();
            return host;
        }
    }
}