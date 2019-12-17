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
            var totalRank = couple.Babies.OfType<LuckyBaby>().Sum(b => b.Rank);
            return (int)couple.Kisses % 10 == 7 ? current + totalRank * .75 : current;
        }
    }
}