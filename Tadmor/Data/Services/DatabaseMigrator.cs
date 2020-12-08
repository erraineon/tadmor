using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Tadmor.Data.Services
{
    public class DatabaseMigrator : IHostedService
    {
        private readonly TadmorDbContext _dbContext;

        public DatabaseMigrator(TadmorDbContext dbContext)
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