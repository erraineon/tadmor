using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseModule<TModule>(this IHostBuilder hostBuilder)
            where TModule : ModuleBase<ICommandContext>
        {
            return hostBuilder.ConfigureServices(UseModule<TModule>);
        }

        public static void UseModule<TModule>(this IServiceCollection services)
            where TModule : ModuleBase<ICommandContext>
        {
            var moduleType = typeof(TModule);
            var moduleAlreadyAdded = services
                .Any(s => s.ServiceType == typeof(IModuleRegistration) &&
                          s.ImplementationInstance is ModuleRegistration registration &&
                          registration.ModuleType == moduleType);
            if (!moduleAlreadyAdded)
                services.AddSingleton<IModuleRegistration>(new ModuleRegistration(moduleType));
        }
    }
}