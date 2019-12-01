using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    public class LoveBaby : Baby, IKissCooldownAffector
    {

        public Task<TimeSpan> CalculateNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown,
            MarriedCouple marriage, IList<IKissCooldownAffector> cooldownAffectors)
        {
            return Task.FromResult(currentCooldown - baseCooldown * 0.1);
        }

        protected override string GetDescription()
        {
            return "reduces kiss cooldown by 10%";
        }
    }
}