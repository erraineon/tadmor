using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    public class AffectionateBaby : Baby
    {
        public override string GetDescription()
        {
            return "add kisses the longer you wait";
        }

        public override Task Release(MarriedCouple marriage)
        {
            var halfRank = Rank / 2f;
            marriage.Kisses += (int) halfRank * halfRank;
            return Task.CompletedTask;
        }
    }
}