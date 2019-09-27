using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Tadmor.Extensions;
using Tadmor.Services.Data;

namespace Tadmor
{
    public class Program
    {
        private static async Task Main(string[] args)
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
                    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {MessageCacheSize = 100}))
                    .Scan(scan => scan
                        .FromEntryAssembly()
                        .AddClasses(classes => classes
                            .Where(type => new[] {"Service", "Job"}.Any(type.Name.EndsWith)))
                        .AsSelfWithInterfaces()
                        .WithSingletonLifetime()))
                .ConfigureLogging(configLogging => configLogging.AddConsole())
                .UseConsoleLifetime()
                .Build();
            return host;
        }
    }
}