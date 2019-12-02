using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    public class KissBaby : Baby, IKissIncrementAffector
    {
        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement,
            MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors)
        {
            return Task.FromResult(currentIncrement + baseKissIncrement * 0.5f);
        }

        public override string GetDescription()
        {
            return "increases kisses";
        }
    }
}