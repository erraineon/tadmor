using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    public class NormalBaby : Baby, IKissIncrementAffector
    {

        public Task<float> CalculateNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors)
        {
            return Task.FromResult(currentIncrement + baseKissIncrement * 0.5f);
        }

        protected override string GetDescription()
        {
            return "increases kisses by 50%";
        }
    }
}
