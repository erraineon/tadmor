using System.Linq;
using MoreLinq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public class SiameseBabyEffector : MarriageEffector, IKissGainEffector
    {
        public SiameseBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            var gainMultiplier = couple.Babies.OfType<SiameseBaby>()
                .Batch(2, babies =>
                {
                    var enumeratedBabies = babies.ToList();
                    var first = enumeratedBabies.First();
                    var second = enumeratedBabies.Last();
                    var isMatched = first != second;
                    return isMatched ? (first.Rank + second.Rank) / 2.8 : -first.Rank / 8.0;
                })
                .Sum();
            return current + seed * gainMultiplier;
        }
    }
}