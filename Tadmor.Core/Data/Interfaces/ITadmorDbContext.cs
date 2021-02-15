using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tadmor.Core.Bookmarks.Models;
using Tadmor.Core.Data.Models;

namespace Tadmor.Core.Data.Interfaces
{
    public interface ITadmorDbContext : IAsyncDisposable
    {
        DbSet<GuildPreferencesEntity> GuildPreferences { get; }
        DbSet<Bookmark> Bookmarks { get; set; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;
    }
}