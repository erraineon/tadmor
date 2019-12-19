using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public class KissBabyEffector : MarriageEffector, IKissGainEffector
    {
        public KissBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            var kissBabies = couple.Babies.OfType<KissBaby>().ToList();
            var multiplier = couple.Babies.OfType<KissBaby>().Sum(b => b.Rank) / 8.0;
            var extraKisses = seed * multiplier;
            if (extraKisses >= 1)
            {
                Logger.Log($"{GetBabyNames(kissBabies)} gave you {extraKisses:0} extra kisses");
            }

            return current + extraKisses;
        }
    }
}