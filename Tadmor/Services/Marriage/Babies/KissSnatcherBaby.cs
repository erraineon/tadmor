using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyEffectOrder(int.MaxValue)]
    [BabyFrequency(0.2f)]
    public class KissSnatcherBaby : Baby, IKissIncrementAffector
    {
        readonly Random _random = new Random();

        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors, ILogger logger)
        {
            var proc = _random.NextDouble() < 0.1;
            if (proc)
            {
                logger.LogInformation($"{Name} nullified your kiss gain");
                return Task.FromResult(0f);
            }

            return Task.FromResult(currentIncrement);
        }

        public override string GetDescription()
        {
            return "has a chance of not letting you get any kisses";
        }

        public override Task Release(MarriedCouple marriage)
        {
            const float releaseRate = 10;
            if (marriage.Kisses < releaseRate) 
                throw new Exception($"you need at least {releaseRate} kisses to release this baby");
            marriage.Kisses -= releaseRate;
            return Task.CompletedTask;
        }
    }
}