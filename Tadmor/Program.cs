using System;
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
using Tadmor.Services.Cron;
using Tadmor.Services.CustomSearch;
using Tadmor.Services.Discord;
using Tadmor.Services.E621;

namespace Tadmor
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                var provider = GetServiceProvider();
                var context = provider.GetService<AppDbContext>();
                await context.Database.MigrateAsync();
                var discord = provider.GetService<DiscordService>();
                await discord.Start();
                var hangfireServer = new BackgroundJobServer(new BackgroundJobServerOptions {WorkerCount = 1});
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        public static ServiceProvider GetServiceProvider()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var services = new ServiceCollection()
                .Configure<DiscordOptions>(configuration.GetSection(nameof(DiscordOptions)))
                .Configure<E621Options>(configuration.GetSection(nameof(E621Options)))
                .Configure<CustomSearchOptions>(configuration.GetSection(nameof(CustomSearchOptions)))
                .AddLogging(logger => logger.AddConsole())
                .AddDbContext<AppDbContext>(builder => builder.UseSqlite(configuration.GetConnectionString("Main")))
                .AddSingleton(new CommandService(new CommandServiceConfig {DefaultRunMode = RunMode.Async}))
                .AddSingleton<DiscordSocketClient>()
                .Scan(scan => scan
                    .FromEntryAssembly()
                    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                    .AsSelf()
                    .WithSingletonLifetime())
                .BuildServiceProvider();
            GlobalConfiguration.Configuration.UseSQLiteStorage(configuration.GetConnectionString("Hangfire"));
            GlobalConfiguration.Configuration.UseActivator(new IocHangfireJobActivator(services));
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute {Attempts = 0}); //don't retry failed jobs
            return services;
        }
    }
}