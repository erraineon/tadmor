using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.Adapters.Telegram;
using Tadmor.Extensions;
using Tadmor.Logging;
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
                    .AddJsonFile("sonagen.json")
                    .AddJsonFile("compliments.json", false))
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
                    .AddScoped<StringLogger>()
                    .AddScoped<CommandContextResolver>()
                    .AddScoped(p => p.GetService<CommandContextResolver>().GetCommandContext())
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
                .ConfigureLogging(configLogging => configLogging
                    .AddConsole()
                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None))
                .UseConsoleLifetime()
                .Build();
            return host;
        }
    }

    public class CommandContextResolver
    {
        public ICommandContext? CurrentCommandContext { get; set; }
        public ICommandContext? GetCommandContext()
        {
            return CurrentCommandContext;
        }
    }
}