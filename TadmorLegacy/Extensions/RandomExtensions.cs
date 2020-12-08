using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Tadmor.Extensions
{
    public static class RandomExtensions
    {
        public static Random ToRandom(this object seed)
        {
            var serializedSeed = JsonConvert.SerializeObject(seed).ToLower();
            var hash = serializedSeed.Aggregate(default(uint), (i, c) => (i + c) * 2654435761);
            return new Random((int) hash);
        }

        public static T Random<T>(this IEnumerable<T> sequence, Func<T, float> weightFunc, Random random)
        {
            IEnumerable<T> RandomSubset(int subsetSize)
            {
                var weightAccumulator = 0f;
                var itemsAndWeight = sequence
                    .Select(item => (item, weight: weightAccumulator += weightFunc(item)))
                    .ToList();
                var totalWeight = itemsAndWeight.Last().weight;
                for (var i = 0; i < subsetSize; i++)
                {
                    var weightIndex = random.NextDouble() * totalWeight;
                    yield return itemsAndWeight.First(t => t.weight >= weightIndex).item;
                }
            }

            return RandomSubset(1).First();
        }
    }
}