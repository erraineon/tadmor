using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MoreLinq;
using Tadmor.Utils;

namespace Tadmor.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        ///     automatically map all subsections to types with the same name
        /// </summary>
        public static IServiceCollection AddWritableOptions(this IServiceCollection services,
            IConfiguration configuration, string optionsPath)
        {
            // get generic method to configure writable options for a given section
            const BindingFlags methodFlags = BindingFlags.NonPublic | BindingFlags.Static;
            var openMethod = typeof(ServiceExtensions).GetMethod(nameof(ConfigureWritable), methodFlags) ??
                             throw new Exception("generic options configuration method not found");

            // map all configuration sections to types by their name, construct the configuration method and invoke it
            configuration.GetChildren()
                .Join(Assembly.GetCallingAssembly().GetExportedTypes(),
                    section => section.Key,
                    type => type.Name,
                    (section, type) => openMethod
                        .MakeGenericMethod(type)
                        .Invoke(null, new object[] {services, section, optionsPath}))
                .Consume();
            return services;
        }

        private static void ConfigureWritable<T>(IServiceCollection services, IConfigurationSection section,
            string optionsPath) where T : class, new()
        {
            services
                .Configure<T>(section)
                .AddTransient<IWritableOptionsSnapshot<T>>(provider =>
                {
                    var environment = provider.GetService<IHostEnvironment>();
                    var options = provider.GetService<IOptionsSnapshot<T>>();
                    return new WritableOptionsSnapshot<T>(environment, options, section.Key, optionsPath);
                });
        }
    }
}