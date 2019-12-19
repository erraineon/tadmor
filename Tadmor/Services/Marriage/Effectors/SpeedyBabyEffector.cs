using System;
using System.Linq;
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
            var totalRank = couple.Babies.OfType<SpeedyBaby>().Sum(b => b.Rank);
            var reduction = .66 * (totalRank / (totalRank + 10.0));
            return current - seed * reduction;
        }
    }
}