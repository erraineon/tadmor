using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage, IList<IKissIncrementAffector> kissAffectors)
        {
            return Task.FromResult(currentIncrement + (int) ((DateTime.Now - marriage.LastKissed).TotalHours / 6));
        }
    }
}