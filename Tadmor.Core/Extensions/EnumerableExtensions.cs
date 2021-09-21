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

            var itemsAndWeight = values
                .Select(item => (item, weight: weightFunction(item)))
                .Where(t => t.weight > 0)
                .ToList();

            for (var i = 0; i < subsetSize; i++)
            {
                if (itemsAndWeight.Any())
                {
                    var totalWeight = itemsAndWeight.Sum(t => t.weight);
                    var minimumWeightToPick = random.NextDouble() * totalWeight;
                    var weightAccumulator = 0f;
                    var (selectedItem, _, selectedIndex) = itemsAndWeight
                        .Select((t, index) => (t.item, t.weight, index))
                        .SkipWhile(t => (weightAccumulator += t.weight) < minimumWeightToPick)
                        .First();
                    yield return selectedItem;
                    itemsAndWeight.RemoveAt(selectedIndex);
                }
            }
        }

        public static TValue? RandomOrDefault<TValue>(
            this IEnumerable<TValue> values, 
            Random? random = null,
            Func<TValue, float>? weightFunction = null)
        {
            return RandomSubset(values, 1, random, weightFunction).FirstOrDefault();
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