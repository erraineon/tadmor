using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    public class AffectionateBaby : Baby, IKissIncrementAffector
    {
        public override string GetDescription()
        {
            return "add kisses the longer you wait";
        }

        public override Task Release(MarriedCouple marriage)
        {
            var halfRank = Rank / 2f;
            marriage.Kisses += (int) halfRank * halfRank;
            return Task.CompletedTask;
        }

        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors, ILogger logger)
        {
            var hoursWaited = (int)(DateTime.Now - marriage.LastKissed).TotalHours;
            var extraKisses = hoursWaited * Rank / 12;
            if (extraKisses > 0)
                logger.LogInformation($"{Name} gave you {extraKisses} extra " +
                                      $"kisses for having waited {hoursWaited} hours");
            return Task.FromResult(currentIncrement + extraKisses);
        }
    }
}