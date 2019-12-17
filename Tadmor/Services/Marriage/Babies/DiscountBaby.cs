using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    [BabyFrequency(0.5f)]
    public class DiscountBaby : Baby
    {
        public override string GetDescription()
        {
            return "has a chance of halving the cost of new babies";
        }

        public override Task Release(MarriedCouple marriage)
        {
            var doubleRank = Rank * 2;
            marriage.Kisses += doubleRank * doubleRank;
            return Task.CompletedTask;
        }
    }
}