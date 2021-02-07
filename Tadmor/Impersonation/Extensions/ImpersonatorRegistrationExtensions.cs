using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
using Tadmor.Impersonation.Interfaces;
using Tadmor.Impersonation.Services;

namespace Tadmor.Impersonation.Extensions
{
    public static class ImpersonatorRegistrationExtensions
    {
        public static IHostBuilder UseImpersonator(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services.AddTransient<IImpersonator, Impersonator>())
                .UseModule<ImpersonationModule>();
        }
    }
}