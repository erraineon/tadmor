using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Tadmor.Services.Data
{
    [SingletonService]
    public class DataService : IHostedService
    {
        private readonly AppDbContext _context;

        public DataService(AppDbContext context)
        {
            _context = context;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _context.Database.MigrateAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _context.Dispose();
            return Task.CompletedTask;
        }
    }
}