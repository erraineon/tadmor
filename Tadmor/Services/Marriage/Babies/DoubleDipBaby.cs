using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyEffectOrder(int.MaxValue)]
    [BabyFrequency(0.2f)]
    public class DoubleDipBaby : Baby, IKissCooldownAffector
    {
        readonly Random _random = new Random();

        public override string GetDescription()
        {
            return "has a chance of resetting your cooldown";
        }

        public Task<TimeSpan> GetNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown, MarriedCouple marriage, IList<IKissCooldownAffector> cooldownAffectors)
        {
            return Task.FromResult(_random.NextDouble() < 0.1 ? TimeSpan.Zero : currentCooldown);
        }
    }
}