using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    public class KissBaby : Baby, IKissIncrementAffector
    {
        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement,
            MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors, ILogger logger)
        {
            return Task.FromResult(currentIncrement + baseKissIncrement * 0.5f);
        }

        public override string GetDescription()
        {
            return "increases kisses";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += 2;
            return Task.CompletedTask;
        }
    }
}