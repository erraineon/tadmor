using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Tadmor.Commands.Modules;
using Tadmor.Data;
using Tadmor.Data.Interfaces;
using Tadmor.Data.Services;
using Tadmor.Extensions;
using Tadmor.Preference.Modules;
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

        public static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
            .ConfigureAppConfiguration(config => config
                .AddJsonFile("appsettings.json"))
            .UseDiscord()
            .UseTelegram()
            .UseLogging()
            .UseCommands()
            .UseScheduledTasks()
            .UseModule<TestModule>()
            .UseModule<CommandPreferencesModule>()
            .UseModule<RulePreferencesModule>()
            .UseConsoleLifetime()
            .ConfigureServices(s =>
            {
                s.AddDbContext<TadmorDbContext>();
                s.TryAddScoped<ITadmorDbContext, TadmorDbContext>();
            });
    }
}