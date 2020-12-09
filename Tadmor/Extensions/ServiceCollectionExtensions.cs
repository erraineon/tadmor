using System;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tadmor.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection BindConfigurationSection<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration) where TOptions : class
        {
            var options = (TOptions) FormatterServices.GetUninitializedObject(typeof(TOptions));
            configuration.GetSection(typeof(TOptions).Name).Bind(options);
            services.TryAddSingleton(options);
            return services;
        }

        public static void Decorate<TInterface, TDecorator>(this IServiceCollection services)
            where TInterface : class
            where TDecorator : class, TInterface
        {
            var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TInterface));

            if (wrappedDescriptor != null)
            {
                var objectFactory = ActivatorUtilities.CreateFactory(
                    typeof(TDecorator),
                    new[] {typeof(TInterface)});

                services.Replace(ServiceDescriptor.Describe(
                    typeof(TInterface),
                    s => (TInterface) objectFactory(s, new[] {s.CreateInstance(wrappedDescriptor)}),
                    wrappedDescriptor.Lifetime)
                );
            }
        }

        private static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (descriptor.ImplementationFactory != null)
                return descriptor.ImplementationFactory(services);

            return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType);
        }
    }
}