using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tadmor.Core.Data.Models;

namespace Tadmor.Core.Data.Interfaces
{
    public interface ITadmorDbContext : IAsyncDisposable
    {
        DbSet<GuildPreferencesEntity> GuildPreferences { get; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}