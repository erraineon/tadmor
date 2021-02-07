using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Data.Interfaces;

namespace Tadmor.Core.Data.Services
{
    [ExcludeFromCodeCoverage]
    public class DatabaseMigrator : IHostedService
    {
        private readonly ITadmorDbContext _dbContext;

        public DatabaseMigrator(ITadmorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
            await _dbContext.DisposeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}