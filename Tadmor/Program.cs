using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using Hangfire.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tadmor.Data;
using Tadmor.Extensions;
using Tadmor.Services.Cron;
using Tadmor.Services.Discord;

namespace Tadmor
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
            var discord = services.GetRequiredService<DiscordService>();
            await discord.Start();
            services.GetService<IGlobalConfiguration>();
            //WorkerCount must be one when using sqlite or jobs will fire multiple times
            var hangfireServer = new BackgroundJobServer(new BackgroundJobServerOptions {WorkerCount = 1});
            await Task.Delay(-1);
        }

        public static ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("sonagen.json")
                .Build();

            var services = new ServiceCollection()
                .Configure(configuration)
                .AddLogging(logger => logger.AddConsole())
                .AddDbContext<AppDbContext>(builder => builder.UseSqlite(configuration.GetConnectionString("Main")))
                .AddSingleton(new CommandService(new CommandServiceConfig {DefaultRunMode = RunMode.Async}))
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {MessageCacheSize = 100}))
                .Scan(scan => scan
                    .FromEntryAssembly()
                    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                    .AsSelf()
                    .WithSingletonLifetime())
                .BuildServiceProvider();

            GlobalConfiguration.Configuration
                .UseActivator(new InjectedJobActivator(services))
                .UseSQLiteStorage(configuration.GetConnectionString("Hangfire"))
                .UseFilter(new AutomaticRetryAttribute {Attempts = 0});
            return services;
        }
    }
}