using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public class LuckyBabyEffector : MarriageEffector, IKissGainEffector
    {
        public LuckyBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            var luckyBabies = couple.Babies.OfType<LuckyBaby>().ToList();
            var totalRank = luckyBabies.Sum(b => b.Rank);
            var currentKisses = couple.Kisses;
            if ((int) currentKisses % 10 == 7)
            {
                var extraKisses = totalRank * .75;
                Logger.Log($"{GetBabyNames(luckyBabies)} gave you {extraKisses} extra kisses for having {currentKisses} kisses");
                return current + extraKisses;
            }
            return current;

        }
    }
}