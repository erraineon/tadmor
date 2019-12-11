using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyEffectOrder(int.MaxValue)]
    [BabyFrequency(0.07f)]
    public class CockBlockBaby : Baby, IKissCooldownAffector, IKissIncrementAffector
    {
        public override string GetDescription()
        {
            return IsPowerBaby
                ? "doubles your kiss cooldown and multiplies your kiss yield by 8"
                : "doubles your kiss cooldown";
        }

        private bool IsPowerBaby => Rank == 10;

        public override Task Release(MarriedCouple marriage)
        {
            if (!IsPowerBaby) marriage.KissCooldown = TimeSpan.FromDays(7);
            return Task.CompletedTask;
        }

        public Task<TimeSpan> GetNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown, MarriedCouple marriage, 
            IList<IKissCooldownAffector> cooldownAffectors, ILogger logger)
        {
            logger.LogInformation($"{Name} doubled your cooldown");
            return Task.FromResult(currentCooldown*2);
        }

        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage, IList<IKissIncrementAffector> kissAffectors,
            ILogger logger)
        {
            if (IsPowerBaby)
            {
                logger.LogInformation($"{Name} multiplied your kisses by 8");
                return Task.FromResult(currentIncrement * 8);
            }

            return Task.FromResult(currentIncrement);
        }
    }
}