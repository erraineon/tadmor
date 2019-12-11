using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    public class SpeedyBaby : Baby, IKissCooldownAffector
    {
        public Task<TimeSpan> GetNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown,
            MarriedCouple marriage, IList<IKissCooldownAffector> cooldownAffectors, ILogger logger)
        {
            var speedyBabies = cooldownAffectors.OfType<SpeedyBaby>().ToList();
            var babiesCount = (float)speedyBabies.Count;
            var babiesRank = speedyBabies.Sum(b => b.Rank / 2f);
            // parabola with asymptote at y = .66
            var reduction = .66 * babiesRank / ((babiesRank + 10) * babiesCount);
            return Task.FromResult(currentCooldown - baseCooldown * reduction);
        }

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