using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyFrequency(0.5f)]
    public class DiscountBaby : Baby, IBabyCostAffector
    {
        readonly Random _random = new Random();

        public override string GetDescription()
        {
            return "has a chance of halving the cost of new babies";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += 3;
            return Task.CompletedTask;
        }

        public Task<float> GetNewCost(float currentCost, float baseCost, MarriedCouple marriage,
            IList<IBabyCostAffector> costAffectors, ILogger logger)
        {
            var proc = _random.NextDouble() < 0.1;
            if (proc)
            {
                var newCost = currentCost / 2;
                logger.LogInformation($"{Name} decreased your cost to {newCost}");
                return Task.FromResult(newCost);
            }

            return Task.FromResult(currentCost);
        }
    }
}