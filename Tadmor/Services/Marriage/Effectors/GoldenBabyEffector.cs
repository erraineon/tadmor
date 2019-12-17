using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    [EffectorOrder(int.MaxValue)]
    public class GoldenBabyEffector : MarriageEffector, IKissGainEffector
    {
        public GoldenBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            return current * (couple.Babies.OfType<GoldenBaby>().Count() + 1);
        }
    }
}