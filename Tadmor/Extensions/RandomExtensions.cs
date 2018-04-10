using System;

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
    }
}