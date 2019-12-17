using System;
using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    [EffectorOrder(int.MaxValue)]
    public class KissSnatcherEffector : MarriageEffector, IKissGainEffector
    {
        public KissSnatcherEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            var babies = couple.Babies.OfType<KissSnatcherBaby>().ToList();
            var totalRank = (double)babies.Sum(b => b.Rank);
            var resetChance = 0.5 * (totalRank / (totalRank + 10));
            var random = new Random();
            if (random.NextDouble() < resetChance)
            {
                Logger.Log($"{GetBabyNames(babies)} nullified your kiss gain");
                return 0;
            }

            return current;
        }
    }
}