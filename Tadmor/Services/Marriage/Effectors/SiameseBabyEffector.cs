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
            var (matched, unmatched) = couple.Babies.OfType<SiameseBaby>()
                .Batch(2, babies =>
                {
                    var enumeratedBabies = babies.ToList();
                    var first = enumeratedBabies.First();
                    var second = enumeratedBabies.Last();
                    var isMatched = first != second;
                    return (first, second, isMatched);
                })
                .Partition(t => t.isMatched, (m, u) => (m.ToList(), u.ToList()));
            var extraKisses = matched.Sum(t => seed * ((t.first.Rank + t.second.Rank) / 2.8));
            var lostKisses = unmatched.Sum(t => seed * (t.first.Rank / 8.0));
            if (matched.Any())
            {
                var allMatchedBabies = matched.SelectMany(t => new []{t.first, t.second});
                Logger.Log($"{GetBabyNames(allMatchedBabies)} gave you {extraKisses:0} extra kisses for being matched");
            }

            if (unmatched.Any())
            {
                var allUnmatchedBabies = unmatched.Select(t => t.first);
                Logger.Log($"{GetBabyNames(allUnmatchedBabies)} stole {lostKisses:0} kisses for being unmatched");
            }
            return current + extraKisses - lostKisses;
        }
    }
}