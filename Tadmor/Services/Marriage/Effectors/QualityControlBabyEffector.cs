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
            var totalRank = couple.Babies.OfType<QualityControlBaby>().Sum(b => b.Rank);
            var extraBonus = .1 * (totalRank / (totalRank + 10.0));
            return extraBonus;
        }
    }
}