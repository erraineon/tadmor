using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Abstractions.Extensions;
using Tadmor.Core.ChatClients.Discord.Extensions;
using Tadmor.Core.ChatClients.Telegram.Extensions;
using Tadmor.Core.Commands.Extensions;
using Tadmor.Core.Extensions;
using Tadmor.GuildManager.Extensions;

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
                .UseModule<TestModule>()
                .UseGuildManager()
                .UseConsoleLifetime();
        }
    }
}