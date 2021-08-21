using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Data.Interfaces;
using Tadmor.Core.Data.Services;

namespace Tadmor.Core.Data.Extensions
{
    public static class DataRegistrationExtensions
    {
        public static IHostBuilder UseTadmorDbContext(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(
                    (hostingContext, services) =>
                    {
                        var configuration = hostingContext.Configuration;
                        var connectionString = configuration.GetConnectionString("SqliteConnectionString");
                        services
                            .AddDbContext<TadmorDbContext>(optionsBuilder => optionsBuilder.UseSqlite(connectionString))
                            .AddHostedService<DatabaseMigrator>()
                            .TryAddScoped<ITadmorDbContext, TadmorDbContext>();
                    });
        }
    }
}