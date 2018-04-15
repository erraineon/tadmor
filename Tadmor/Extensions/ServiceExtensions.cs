using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tadmor.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        ///     automatically map all subsections to types with the same name
        /// </summary>
        public static ServiceCollection Configure(this ServiceCollection services, IConfiguration configuration)
        {
            var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                    new[] {typeof(ServiceCollection), typeof(IConfigurationRoot)});
            var publicTypes = Assembly.GetEntryAssembly().GetExportedTypes();
            var sectionNamesAndOptionTypes = from section in configuration.GetChildren()
                let name = section.Key
                let type = publicTypes.SingleOrDefault(type => type.Name == name)
                where type != null
                select (name, type);
            foreach (var (name, type) in sectionNamesAndOptionTypes)
            {
                var genericConfigureMethod = configureMethod.MakeGenericMethod(type);
                genericConfigureMethod.Invoke(null, new object[] {services, configuration.GetSection(name)});
            }

            return services;
        }
    }
}