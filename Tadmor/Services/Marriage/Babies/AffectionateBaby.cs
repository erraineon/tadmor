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
            return "adds a kiss for each 6 hours since your last kiss";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += 2;
            return Task.CompletedTask;
        }

        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors, ILogger logger)
        {
            var hoursWaited = (int)(DateTime.Now - marriage.LastKissed).TotalHours;
            var extraKisses = hoursWaited / 6;
            if (extraKisses > 0)
                logger.LogInformation($"{Name} gave you {extraKisses} extra " +
                                      $"kisses for having waited {hoursWaited} hours");
            return Task.FromResult(currentIncrement + extraKisses);
        }
    }
}