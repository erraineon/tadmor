using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    [BabyFrequency(0.03f)]
    public class GoldenBaby : Baby
    {
        public override string GetDescription()
        {
            return "doubles your kisses";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += 10 * Rank;
            return Task.CompletedTask;
        }
    }
}