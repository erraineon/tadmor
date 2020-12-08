using Microsoft.EntityFrameworkCore;
using Tadmor.Data.Models;
using Tadmor.Extensions;

namespace Tadmor.Data.Services
{
    public class TadmorDbContext : DbContext
    {
        public DbSet<GuildPreferencesEntity> GuildPreferences { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildPreferencesEntity>(b =>
            {
                b.HasKey(e => e.GuildId);
                b.Property(e => e.GuildId).ValueGeneratedNever();
                b.Property(e => e.Preferences).HasJsonConversion();
            });
            base.OnModelCreating(modelBuilder);
        }

        public TadmorDbContext(DbContextOptions<TadmorDbContext> options) : base(options)
        {
        }
    }
}