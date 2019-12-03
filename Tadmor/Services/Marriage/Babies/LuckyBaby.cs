using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyFrequency(0.2f)]
    public class LuckyBaby : Baby, IKissIncrementAffector
    {
        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors)
        {
            return Task.FromResult((int) marriage.Kisses % 10 == 7 ? currentIncrement + 3 : currentIncrement);
        }

        public override string GetDescription()
        {
            return "if the last digit of your kisses is 7, add 3 extra kisses";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += 7;
            return Task.CompletedTask;
        }
    }
}