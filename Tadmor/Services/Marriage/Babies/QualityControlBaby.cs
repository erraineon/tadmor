using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyFrequency(0.07f)]
    public class QualityControlBaby : Baby, IBabyRankBonusAffector
    {
        public override string GetDescription()
        {
            return "increases your baby quality";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += Rank * 10;
            return Task.CompletedTask;
        }

        public Task<float> GetNewBabyRankBonus(float currentRankBonus, float rankBonus, MarriedCouple marriage, 
            List<IBabyRankBonusAffector> babyRankBonusAffectors, ILogger logger)
        {
            var qcBabies = babyRankBonusAffectors.OfType<QualityControlBaby>().ToList();
            var babiesCount = (float)qcBabies.Count;
            var babiesRank = qcBabies.Sum(b => b.Rank/2f);
            // parabola with asymptote at y = .1
            var extraBonus = .1 * babiesRank / (babiesRank + 10) / babiesCount;
            return Task.FromResult(currentRankBonus + (float)extraBonus);
        }
    }
}