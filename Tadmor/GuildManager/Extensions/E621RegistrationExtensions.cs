using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
using Tadmor.Furry.Services;
using Tadmor.GuildManager.Interfaces;
using Tadmor.GuildManager.Modules;
using Tadmor.GuildManager.Services;

namespace Tadmor.GuildManager.Extensions
{
    public static class E621RegistrationExtensions
    {
        public static IHostBuilder UseE621(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddTransient<IE621Client, E621ClientWrapper>();
                })
                .UseModule<GuildManagerModule>();
        }
    }
}