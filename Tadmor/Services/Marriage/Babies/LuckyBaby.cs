using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    [BabyFrequency(0.6f)]
    public class LuckyBaby : Baby
    {
        public override string GetDescription()
        {
            return "if the last digit of your kisses is 7, add extra kisses";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += Rank * 1.75f;
            return Task.CompletedTask;
        }
    }
}