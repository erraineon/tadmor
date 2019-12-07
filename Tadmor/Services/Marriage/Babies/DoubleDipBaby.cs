using System    ;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    [BabyEffectOrder(int.MaxValue)]
    [BabyFrequency(0.2f)]
    public class DoubleDipBaby : Baby, IKissCooldownAffector
    {
        readonly Random _random = new Random();

        public override string GetDescription()
        {
            return "has a chance of resetting your cooldown";
        }

        public override Task Release(MarriedCouple marriage)
        {
            marriage.Kisses += 6;
            marriage.KissCooldown = TimeSpan.Zero;
            return Task.CompletedTask;
        }

        public Task<TimeSpan> GetNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown, MarriedCouple marriage, 
            IList<IKissCooldownAffector> cooldownAffectors, ILogger logger)
        {
            var proc = _random.NextDouble() < 0.1;
            if (proc)
            {
                logger.LogInformation($"{Name} nullified your cooldown");
                return Task.FromResult(TimeSpan.Zero);
            }

            return Task.FromResult(currentCooldown);
        }
    }
}