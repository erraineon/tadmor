using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    [BabyFrequency(0.07f)]
    public class QualityControlBaby : Baby
    {
        public override string GetDescription()
        {
            return "increases your baby quality";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += Rank * 10;
            return Task.CompletedTask;
        }
    }
}