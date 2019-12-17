using System;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    [BabyFrequency(0.07f)]
    public class CockBlockBaby : Baby
    {
        public override string GetDescription()
        {
            return "doubles your kiss cooldown";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.KissCooldown += TimeSpan.FromDays(7);
            return Task.CompletedTask;
        }

        public override Task ExecuteCombinePrecondition(MarriedCouple marriage)
        {
            marriage.KissCooldown += TimeSpan.FromDays(10);
            return Task.CompletedTask;
        }
    }
}