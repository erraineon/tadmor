using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Tadmor.Core.Bookmarks.Models;
using Tadmor.Core.Data.Interfaces;
using Tadmor.Core.Data.Models;

namespace Tadmor.Core.Data.Services
{
    [ExcludeFromCodeCoverage]
    public class TadmorDbContext : DbContext, ITadmorDbContext
    {
        public TadmorDbContext(DbContextOptions<TadmorDbContext> options) : base(options)
        {
        }

        public DbSet<GuildPreferencesEntity> GuildPreferences { get; set; } = null!;
        public DbSet<Bookmark> Bookmarks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildPreferencesEntity>(b =>
            {
                b.HasKey(e => e.GuildId);
                b.Property(e => e.GuildId).ValueGeneratedNever();
                HasJsonConversion(b.Property(e => e.Preferences));
            });
            modelBuilder.Entity<Bookmark>(b =>
            {
                b.HasKey(e => new {e.ChatClientId, e.GuildId, e.ChannelId, e.Key});
            });
            base.OnModelCreating(modelBuilder);
        }

        private static void HasJsonConversion<T>(PropertyBuilder<T> propertyBuilder) where T : class, new()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.Auto
            };

            ValueConverter<T, string> converter = new(
                v => JsonConvert.SerializeObject(v, jsonSerializerSettings),
                v => JsonConvert.DeserializeObject<T>(v, jsonSerializerSettings) ?? new T()
            );

            ValueComparer<T> comparer = new(
                (l, r) => JsonConvert.SerializeObject(l, jsonSerializerSettings) ==
                    JsonConvert.SerializeObject(r, jsonSerializerSettings),
                v => v == null ? 0 : JsonConvert.SerializeObject(v, jsonSerializerSettings).GetHashCode(),
                v => JsonConvert.DeserializeObject<T>(
                    JsonConvert.SerializeObject(v, jsonSerializerSettings),
                    jsonSerializerSettings)
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);
        }
    }
}