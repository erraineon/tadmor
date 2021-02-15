using System;
using System.Collections.Generic;
using System.Linq;

namespace Tadmor.Core.Extensions
{
    public static class EnumerableExtensions
    {
        private static readonly Random DefaultRandom = new();

        public static IEnumerable<TValue> RandomSubset<TValue>(this IEnumerable<TValue> values,
            int subsetSize,
            Random? random = null,
            Func<TValue, float>? weightFunction = null)
        {
            random ??= DefaultRandom;
            weightFunction ??= _ => 1;

            var weightAccumulator = 0f;
            var itemsAndWeight = values
                .Select(item => (item, weight: weightAccumulator += weightFunction(item)))
                .ToList();
            if (!itemsAndWeight.Any())
                throw new ArgumentException("the collection must not be empty to pick a random value", nameof(values));
            var totalWeight = itemsAndWeight.Last().weight;
            for (var i = 0; i < subsetSize; i++)
            {
                var weightIndex = random.NextDouble() * totalWeight;
                yield return itemsAndWeight.First(t => t.weight >= weightIndex).item;
            }
        }

        public static TValue Random<TValue>(
            this IEnumerable<TValue> values, 
            Random? random = null,
            Func<TValue, float>? weightFunction = null)
        {
            return RandomSubset(values, 1, random, weightFunction).First();
        }
    }
}