using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public class QualityControlBabyEffector : MarriageEffector, IBabyRankBonusEffector
    {
        public QualityControlBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            var qualityControlBabies = couple.Babies.OfType<QualityControlBaby>().ToList();
            var totalRank = qualityControlBabies.Sum(b => b.Rank);
            var extraBonus = .8 * (totalRank / (totalRank + 30.0));
            if (qualityControlBabies.Any())
            {
                Logger.Log($"{GetBabyNames(qualityControlBabies)} gave you a {extraBonus:P0} quality bonus");
            }
            return extraBonus;
        }
    }
}