using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyEffectOrder(int.MaxValue)]
    [BabyFrequency(0.07f)]
    public class GoldenBaby : Baby, IKissIncrementAffector
    {
        public override string GetDescription()
        {
            return "doubles your kisses";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += 20;
            return Task.CompletedTask;
        }

        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors, ILogger logger)
        {
            return Task.FromResult(currentIncrement*2);
        }
    }
}