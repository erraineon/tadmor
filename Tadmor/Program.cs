using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using Hangfire.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Scrutor;
using Tadmor.Extensions;
using Tadmor.Services.Data;
using Tadmor.Services.Discord;
using Tadmor.Services.E621;
using Tadmor.Services.Hangfire;

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
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configApp => configApp
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile("sonagen.json"))
                .ConfigureHostConfiguration(configApp => configApp
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile("sonagen.json"))
                .ConfigureServices((hostContext, services) =>
                {
                    //((ServiceCollection)services).Configure(hostContext.Configuration)
                    services
                        .AddOptions(hostContext.Configuration)
                        .AddLogging()
                        .AddDbContext<AppDbContext>(builder => builder
                            .UseSqlite(hostContext.Configuration.GetConnectionString("Main")))
                        .AddHostedService<DataService>()
                        .AddHostedService<HangfireService>()
                        .AddHostedService<DiscordService>()
                        .AddSingleton(new CommandService(new CommandServiceConfig {DefaultRunMode = RunMode.Async}))
                        .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {MessageCacheSize = 100}))
                        .Scan(scan => scan
                            .FromEntryAssembly()
                            .AddClasses(classes => classes
                                .Where(type => new[] {"Service", "Job"}.Any(type.Name.EndsWith)))
                            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                            .AsSelf()
                            .WithSingletonLifetime());
                })
                .ConfigureLogging(configLogging => configLogging.AddConsole())
                .UseConsoleLifetime()
                .Build();
            return host;
        }

        public static async Task UpdateOptions<TSection>(TSection section) where TSection : class, new()
        {
            var settingsPath = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
                .GetFileInfo("appsettings.json")
                .PhysicalPath;
            var jo = JObject.Parse(await File.ReadAllTextAsync(settingsPath));
            jo[typeof(TSection).Name] = JToken.FromObject(section);
            await File.WriteAllTextAsync(settingsPath, jo.ToString());
        }
    }
}