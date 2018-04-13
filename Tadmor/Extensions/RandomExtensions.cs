using System;
using System.Collections.Generic;
using System.Linq;

namespace Tadmor.Extensions
{
    public static class RandomExtensions
    {
        public static Random ToRandom(this string value, Random salt = default)
        {
            //.net core adds randomness to hash code generation. use this for fixed behavior
            int GetStableHashCode(string str)
            {
                unchecked
                {
                    var hash1 = 5381;
                    var hash2 = hash1;

                    for (var i = 0; i < str.Length && str[i] != '\0'; i += 2)
                    {
                        hash1 = ((hash1 << 5) + hash1) ^ str[i];
                        if (i == str.Length - 1 || str[i + 1] == '\0')
                            break;
                        hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                    }

                    var saltMultiplier = salt?.Next() ?? 1;
                    return hash1 * saltMultiplier + hash2 * 1566083941;
                }
            }

            var hashCode = GetStableHashCode(value);
            return new Random(hashCode);
        }

        public static T GetRandom<T>(this ICollection<T> sequence, Func<T, float> weightSelector, Random random)
        {
            if (weightSelector == null) weightSelector = arg => 1f;
            var totalWeight = sequence.Sum(weightSelector);
            var nextDouble = random.NextDouble();
            var itemWeightIndex = (float)(nextDouble * totalWeight);
            float currentWeightIndex = 0;

            foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
            {
                currentWeightIndex += item.Weight;
                if (currentWeightIndex >= itemWeightIndex) return item.Value;
            }
            return default;
        }

        public static bool RandomByWeight(this float weight, Random random)
        {
            return (float)random.NextDouble() < weight;
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Random random)
        {
            return source.OrderBy(e => random.Next());
        }
    }
}