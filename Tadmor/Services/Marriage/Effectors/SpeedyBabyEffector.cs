using System;
using System.Linq;
using Humanizer;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public class SpeedyBabyEffector : MarriageEffector, IKissCooldownEffector
    {
        public SpeedyBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public TimeSpan GetNewValue(TimeSpan current, TimeSpan seed, MarriedCouple couple)
        {
            var speedyBabies = couple.Babies.OfType<SpeedyBaby>().ToList();
            var totalRank = speedyBabies.Sum(b => b.Rank);
            var multiplier = .66 * (totalRank / (totalRank + 10.0));
            var reduction = seed * multiplier;
            if (speedyBabies.Any())
            {
                Logger.Log($"{GetBabyNames(speedyBabies)} reduced your cooldown by {reduction.Humanize()}");
            }

            return current - reduction;
        }
    }
}