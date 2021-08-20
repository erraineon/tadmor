using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Bookmarks.Extensions;
using Tadmor.Core.ChatClients.Abstractions.Extensions;
using Tadmor.Core.ChatClients.Discord.Extensions;
using Tadmor.Core.ChatClients.Telegram.Extensions;
using Tadmor.Core.Commands.Extensions;
using Tadmor.Core.Data.Extensions;
using Tadmor.Core.Extensions;
using Tadmor.Furry.Extensions;
using Tadmor.GuildManager.Extensions;
using Tadmor.MessageRendering.Extensions;
using Tadmor.Raffles.Extensions;
using Tadmor.Search.Extensions;
using Tadmor.TextGeneration.Extensions;
using Tadmor.Twitter.Extensions;
using Tadmor.Utilities.Extensions;

namespace Tadmor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
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
                .UseBookmarks()
                .UseModule<TestModule>()
                .UseGuildManager()
                .UseE621()
                .UseGoogleSearch()
                .UseTadmorMind()
                .UseTwitter()
                .UseMessageRendering()
                .UseUtilities()
                .UseRaffles()
                .UseConsoleLifetime();
        }
    }
}