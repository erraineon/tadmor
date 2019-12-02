using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    public class SpeedyBaby : Baby, IKissCooldownAffector
    {

        public Task<TimeSpan> GetNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown,
            MarriedCouple marriage, IList<IKissCooldownAffector> cooldownAffectors)
        {
            var loveBabiesCount = cooldownAffectors.OfType<SpeedyBaby>().Count();
            // parabola with asymptote at y = .66
            var reduction = .66 * loveBabiesCount / ((loveBabiesCount + 10) * loveBabiesCount);
            return Task.FromResult(currentCooldown - baseCooldown * reduction);
        }

        protected override string GetDescription()
        {
            return "reduces kiss cooldown";
        }
    }
}