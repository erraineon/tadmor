using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Tadmor.Data.Interfaces;
using Tadmor.Data.Models;

namespace Tadmor.Data.Services
{
    [ExcludeFromCodeCoverage]
    public class TadmorDbContext : DbContext, ITadmorDbContext
    {
        public TadmorDbContext(DbContextOptions<TadmorDbContext> options) : base(options)
        {
        }

        public DbSet<GuildPreferencesEntity> GuildPreferences { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildPreferencesEntity>(b =>
            {
                b.HasKey(e => e.GuildId);
                b.Property(e => e.GuildId).ValueGeneratedNever();
                HasJsonConversion(b.Property(e => e.Preferences));
            });
            base.OnModelCreating(modelBuilder);
        }

        private static void HasJsonConversion<T>(PropertyBuilder<T> propertyBuilder) where T : class, new()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
            ValueConverter<T, string> converter = new(
                v => JsonConvert.SerializeObject(v, jsonSerializerSettings),
                v => JsonConvert.DeserializeObject<T>(v, jsonSerializerSettings) ?? new T()
            );

            ValueComparer<T> comparer = new(
                (l, r) => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(r, jsonSerializerSettings),
                v => v == null ? 0 : JsonConvert.SerializeObject(v, jsonSerializerSettings).GetHashCode(),
                v => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(v, jsonSerializerSettings))
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);
        }
    }
}