using System;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    [BabyFrequency(0.2f)]
    public class KissSnatcherBaby : Baby
    {
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

        public override Task ExecuteCombinePrecondition(MarriedCouple marriage)
        {
            const float combineRate = 15;
            if (marriage.Kisses < combineRate)
                throw new Exception($"you need at least {combineRate} kisses to combine this baby");
            marriage.Kisses -= combineRate;
            return Task.CompletedTask;
        }
    }
}