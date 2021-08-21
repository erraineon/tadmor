using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Data.Interfaces;
using Tadmor.Core.Extensions;
using Tadmor.Raffles.Data;
using Tadmor.Raffles.Interfaces;
using Tadmor.Raffles.Modules;
using Tadmor.Raffles.Services;

namespace Tadmor.Raffles.Extensions
{
    public static class RafflesRegistrationExtensions
    {
        public static IHostBuilder UseRaffles(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddTransient<IRaffleDrawingService, RaffleDrawingService>();
                    services.AddTransient<IRaffleWinnersRepository, RaffleWinnersRepository>();
                    services.TryAddEnumerable(new[]
                    {
                        ServiceDescriptor.Transient<IModelExtension, RaffleWinnersModelExtension>(),
                        ServiceDescriptor.Transient<IRaffleBiasStrategy, ThreeMonthPenaltyBiasStrategy>(),
                        ServiceDescriptor.Transient<IRaffleBiasStrategy, OnlyNewComersBiasStrategy>()
                    });
                })
                .UseModule<RaffleModule>();
        }
    }
}