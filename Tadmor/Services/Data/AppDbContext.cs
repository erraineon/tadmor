using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Services.Twitter;

namespace Tadmor.Services.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TwitterMedia> TwitterMedia { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TwitterMedia>()
                .HasKey(media => new {media.TweetId, media.MediaId});
        }

        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
        {
            public AppDbContext CreateDbContext(string[] args)
            {
                //ef cli tools will create the db in the project dir without this
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                var services = Program.ConfigureHost().Services;
                return services.GetService<AppDbContext>();
            }
        }
    }
}