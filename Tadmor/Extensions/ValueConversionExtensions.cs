using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Tadmor.Extensions
{
    public static class ValueConversionExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class, new()
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
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

            return propertyBuilder;
        }
    }
}