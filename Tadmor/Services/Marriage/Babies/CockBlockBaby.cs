using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyEffectOrder(int.MaxValue)]
    [BabyFrequency(0.07f)]
    public class CockBlockBaby : Baby, IKissCooldownAffector
    {
        public override string GetDescription()
        {
            return "doubles your kiss cooldown";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.KissCooldown = TimeSpan.FromDays(7);
            return Task.CompletedTask;
        }

        public Task<TimeSpan> GetNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown, MarriedCouple marriage, 
            IList<IKissCooldownAffector> cooldownAffectors, ILogger logger)
        {
            return Task.FromResult(currentCooldown*2);
        }
    }
}