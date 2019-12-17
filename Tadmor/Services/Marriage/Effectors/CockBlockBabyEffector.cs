using System;
using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    [EffectorOrder(int.MaxValue)]
    public class CockBlockBabyEffector : MarriageEffector, IKissCooldownEffector
    {
        public CockBlockBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public TimeSpan GetNewValue(TimeSpan current, TimeSpan seed, MarriedCouple couple)
        {
            var babies = couple.Babies.OfType<CockBlockBaby>().ToList();
            var cooldownMultiplier = babies.Count + 1;
            if (cooldownMultiplier > 1)
            {
                var names = GetBabyNames(babies);
                Logger.Log($"{names} multiplied your cooldown by {cooldownMultiplier}");
            }

            return current * cooldownMultiplier;
        }
    }
}