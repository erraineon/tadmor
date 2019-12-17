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
            return current + seed * (couple.Babies.OfType<KissBaby>().Sum(b => b.Rank) / 8.0);
        }
    }
}