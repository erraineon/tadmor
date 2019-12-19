using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    [BabyFrequency(0.14)]
    public class PatientBaby : Baby
    {
        public override string GetDescription()
        {
            return "increases kisses; if you try kissing before the cooldown is over, halves your total kisses instead and disappears";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += Rank;
            return Task.CompletedTask;
        }
    }
}