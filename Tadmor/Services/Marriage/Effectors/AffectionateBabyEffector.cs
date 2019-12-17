using System;
using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public class AffectionateBabyEffector : MarriageEffector, IKissGainEffector
    {
        public AffectionateBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            var babies = couple.Babies.OfType<AffectionateBaby>().ToList();
            var hoursWaited = (DateTime.Now - couple.LastKissed).TotalHours;
            var hoursMultiplier = hoursWaited / 12;
            var bonus = babies.Sum(b => Math.Floor(b.Rank * hoursMultiplier));
            if (bonus > 0)
            {
                var names = GetBabyNames(babies);
                Logger.Log($"{names} gave you {bonus} kisses for having waited {hoursWaited:0} hours");
            }

            return current + bonus;
        }
    }
}