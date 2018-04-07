using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tadmor.Data;
using Tadmor.Services;

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
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            services
                .Configure<DiscordOptions>(configuration.GetSection(nameof(DiscordOptions)))
                .AddLogging(logger => logger.AddConsole())
                .AddDbContext<AppDbContext>(builder => builder.UseSqlite(configuration.GetConnectionString("Main")))
                .AddSingleton(new CommandService(new CommandServiceConfig {DefaultRunMode = RunMode.Async}))
                .Scan(scan => scan
                    .AddTypes(typeof(DiscordSocketClient))
                    .FromEntryAssembly()
                    .AddClasses(classes => classes.InNamespaceOf<DiscordService>())
                    .AsSelf()
                    .WithSingletonLifetime());
            return services.BuildServiceProvider();
        }
    }
}