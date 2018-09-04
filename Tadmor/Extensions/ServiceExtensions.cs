using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;

namespace Tadmor.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        ///     automatically map all subsections to types with the same name
        /// </summary>
        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            var openMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                    new[] {typeof(ServiceCollection), typeof(IConfigurationRoot)});
            var publicTypes = Assembly.GetEntryAssembly().GetExportedTypes();
            var closedMethods = from section in configuration.GetChildren()
                let name = section.Key
                let type = publicTypes.SingleOrDefault(type => type.Name == name)
                where type != null
                select openMethod.MakeGenericMethod(type).Invoke(null, new object[] {services, section});
            closedMethods.Consume();
            return services;
        }
    }
}