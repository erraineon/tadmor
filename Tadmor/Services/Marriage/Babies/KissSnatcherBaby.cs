using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyEffectOrder(int.MaxValue)]
    [BabyFrequency(0.2f)]
    public class KissSnatcherBaby : Baby, IKissIncrementAffector
    {
        readonly Random _random = new Random();

        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors)
        {
            return Task.FromResult(_random.NextDouble() < 0.1 ? 0f : currentIncrement); 
        }

        protected override string GetDescription()
        {
            return "has a chance of not letting you get any kisses";
        }
    }
}