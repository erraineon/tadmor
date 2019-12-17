using System;
using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public class DiscountBabyEffector : MarriageEffector, ILateBabyCostEffector
    {
        public DiscountBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            var babies = couple.Babies.OfType<DiscountBaby>().ToList();
            var totalRank = babies.Sum(b => b.Rank);
            var discountChance = 0.25 * (totalRank / (totalRank + 5.0));
            var random = new Random();
            if (random.NextDouble() < discountChance)
            {
                var newCost = current / 2;
                Logger.Log($"{GetBabyNames(babies)} decreased your cost to {newCost}");
                return newCost;
            }

            return current;
        }
    }
}