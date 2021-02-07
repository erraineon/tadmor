using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Tadmor.ChatClients.Abstractions.Extensions;
using Tadmor.ChatClients.Discord.Extensions;
using Tadmor.ChatClients.Telegram.Extensions;
using Tadmor.Commands.Extensions;
using Tadmor.Commands.Modules;
using Tadmor.Data.Interfaces;
using Tadmor.Data.Services;
using Tadmor.Extensions;
using Tadmor.GuildManager.Extensions;
using Tadmor.Rules.Extensions;
using Tadmor.Rules.Modules;

namespace Tadmor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                .ConfigureAppConfiguration(
                    config => config
                        .AddJsonFile("appsettings.json"))
                .UseTadmorDbContext()
                .UseDiscord()
                .UseTelegram()
                .UseLogging()
                .UseCommands()
                .UseScheduledTasks()
                .UseModule<TestModule>()
                .UseGuildManager()
                .UseConsoleLifetime();
        }
    }
}