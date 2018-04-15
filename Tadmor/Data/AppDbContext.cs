using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Tadmor.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<GuildOptions> GuildOptions { get; set; }
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
        {
            public AppDbContext CreateDbContext(string[] args)
            {
                //ef cli tools will create the db in the project dir without this
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                var services = Program.ConfigureServices();
                return services.GetService<AppDbContext>();
            }
        }
    }
}