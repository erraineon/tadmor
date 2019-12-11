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
            return Task.FromResult(currentIncrement + baseKissIncrement * (Rank / 8f));
        }

        public override string GetDescription()
        {
            return "increases kisses";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += Rank / 2f;
            return Task.CompletedTask;
        }
    }
}