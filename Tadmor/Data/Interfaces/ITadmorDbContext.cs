using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tadmor.Data.Models;

namespace Tadmor.Data.Interfaces
{
    public interface ITadmorDbContext : IAsyncDisposable
    {
        DbSet<GuildPreferencesEntity> GuildPreferences { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        DatabaseFacade Database { get; }
    }
}