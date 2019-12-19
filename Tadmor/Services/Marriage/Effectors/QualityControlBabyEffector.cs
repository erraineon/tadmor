﻿using System.Linq;
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
            var extraBonus = .1 * (totalRank / (totalRank + 10.0));
            if (qualityControlBabies.Any())
            {
                Logger.Log($"{GetBabyNames(qualityControlBabies)} gave you a {extraBonus:P} quality bonus");
            }
            return extraBonus;
        }
    }
}