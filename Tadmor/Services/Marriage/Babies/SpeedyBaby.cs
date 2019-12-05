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
            var loveBabiesCount = cooldownAffectors.OfType<SpeedyBaby>().Count();
            // parabola with asymptote at y = .66
            var reduction = .66 * loveBabiesCount / ((loveBabiesCount + 10) * loveBabiesCount);
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