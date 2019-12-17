using System;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage
{
    [EffectorOrder(int.MaxValue)]
    [BabyFrequency(0.2f)]
    public class DoubleDipBaby : Baby
    {
        readonly Random _random = new Random();

        public override string GetDescription()
        {
            return "has a chance of resetting your cooldown";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += Rank * 2;
            marriage.KissCooldown = TimeSpan.Zero;
            return Task.CompletedTask;
        }
    }
}