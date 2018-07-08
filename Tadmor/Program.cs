using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using Hangfire.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Tadmor.Data;
using Tadmor.Extensions;
using Tadmor.Services.Cron;
using Tadmor.Services.Discord;

namespace Tadmor
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var dbContext = services.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            var discord = services.GetRequiredService<DiscordService>();
            await discord.StartAsync();
            //WorkerCount must be one when using sqlite or jobs will fire multiple times
            var hangfireServer = new BackgroundJobServer(new BackgroundJobServerOptions {WorkerCount = 1});
            await Task.Delay(-1);
        }

        public static ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
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
                    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Job")))
                    .AsSelf()
                    .WithSingletonLifetime())
                .BuildServiceProvider();

            GlobalConfiguration.Configuration
                .UseActivator(new InjectedJobActivator(services))
                .UseSQLiteStorage(configuration.GetConnectionString("Hangfire"))
                .UseFilter(new AutomaticRetryAttribute {Attempts = 0});
            return services;
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