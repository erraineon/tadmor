using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;

namespace Tadmor.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseModule<TModule>(this IHostBuilder hostBuilder)
            where TModule : ModuleBase<ICommandContext>
        {
            return hostBuilder.ConfigureServices(UseModule<TModule>);
        }

        private static void UseModule<TModule>(this IServiceCollection services)
            where TModule : ModuleBase<ICommandContext>
        {
            services.AddSingleton<IModuleRegistration>(new ModuleRegistration(typeof(TModule)));
        }
    }
}