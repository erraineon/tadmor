using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    public class SpeedyBaby : Baby
    {
        public override string GetDescription()
        {
            return "reduces kiss cooldown";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.KissCooldown /= 2;
            return Task.CompletedTask;
        }
    }
}