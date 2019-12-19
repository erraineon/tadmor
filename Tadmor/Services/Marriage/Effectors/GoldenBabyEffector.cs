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
            var goldenBabies = couple.Babies.OfType<GoldenBaby>().ToList();
            var multiplier = goldenBabies.Count + 1;
            if (goldenBabies.Any())
            {
                Logger.Log($"{GetBabyNames(goldenBabies)} multiplied your kisses by {multiplier}");
            }

            return current * multiplier;
        }
    }
}