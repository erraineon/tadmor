using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
using Tadmor.Utilities.Modules;

namespace Tadmor.Utilities.Extensions
{
    public static class UtilitiesRegistrationExtensions
    {
        public static IHostBuilder UseUtilities(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                })
                .UseModule<UtilitiesModule>();
        }
    }
}