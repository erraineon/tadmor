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
            uint KnuthHash(string str)
            {
                unchecked
                {
                    return str.Aggregate(default(uint), (i, c) => (i + c) * 2654435761);
                }
            }

            var serializedSeed = JsonConvert.SerializeObject(seed).ToLower();
            var hash = KnuthHash(serializedSeed);
            return new Random((int) hash);
        }

        public static IEnumerable<T> RandomSubset<T>(
            this IEnumerable<T> sequence, 
            Func<T, float> weightFunc,
            int subsetSize, 
            Random random)
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

        public static T Random<T>(this IEnumerable<T> sequence, Func<T, float> weightFunc, Random random)
        {
            return sequence.RandomSubset(weightFunc, 1, random).First();
        }
    }
}