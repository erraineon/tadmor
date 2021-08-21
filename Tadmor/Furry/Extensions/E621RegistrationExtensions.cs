using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
using Tadmor.Furry.Interfaces;
using Tadmor.Furry.Modules;
using Tadmor.Furry.Services;

namespace Tadmor.Furry.Extensions
{
    public static class E621RegistrationExtensions
    {
        public static IHostBuilder UseE621(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddTransient<IE621Client, E621ClientWrapper>();
                    services.AddTransient<IE621SearchEngine, E621SearchEngine>();
                })
                .UseModule<FurrySearchEngineModule>();
        }
    }
}