using System;
using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    [EffectorOrder(int.MaxValue)]
    public class DoubleDipBabyEffector : MarriageEffector, IKissCooldownEffector
    {
        public DoubleDipBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public TimeSpan GetNewValue(TimeSpan current, TimeSpan seed, MarriedCouple couple)
        {
            var babies = couple.Babies.OfType<DoubleDipBaby>().ToList();
            var totalRank = (double)babies.Sum(b => b.Rank);
            var resetChance = 0.25 * (totalRank / (totalRank + 5));
            var random = new Random();
            if (random.NextDouble() < resetChance)
            {
                Logger.Log($"{GetBabyNames(babies)} nullified your kiss cooldown");
                return TimeSpan.Zero;
            }

            return current;
        }
    }
}