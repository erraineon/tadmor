using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Services.Marriage;
using Tadmor.Services.Marriage.Babies;
using Tadmor.Services.Twitter;

namespace Tadmor.Services.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TwitterMedia> TwitterMedia { get; set; } = null!;
        public DbSet<MarriedCouple> MarriedCouples { get; set; }
        public DbSet<Baby> Babies { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TwitterMedia>()
                .HasKey(media => new {media.TweetId, media.MediaId, media.Username});
            modelBuilder.Entity<MarriedCouple>()
                .HasIndex(couple => new {couple.Partner1Id, couple.Partner2Id, couple.GuildId})
                .IsUnique();
            modelBuilder.Entity<Baby>()
                .ToTable(nameof(Babies))
                .HasDiscriminator()
                .HasValue<KissBaby>("NormalBaby")
                .HasValue<SpeedyBaby>("LoveBaby")
                .HasValue<KissSnatcherBaby>("EvilBaby")
                .HasValue<DoubleDipBaby>("DoubleDipBaby");
        }

        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
        {
            public AppDbContext CreateDbContext(string[] args)
            {
                //ef cli tools will create the db in the project dir without this
                var services = Program.ConfigureHost().Services;
                var hostEnvironment = services.GetService<IHostEnvironment>();
                Directory.SetCurrentDirectory(hostEnvironment.ContentRootPath);
                return services.GetService<AppDbContext>();
            }
        }
    }
}