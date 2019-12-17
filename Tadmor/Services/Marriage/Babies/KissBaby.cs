using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    public class KissBaby : Baby
    {
        public override string GetDescription()
        {
            return "increases kisses";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += Rank / 2f;
            return Task.CompletedTask;
        }
    }
}