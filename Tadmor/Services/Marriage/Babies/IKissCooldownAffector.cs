using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    public interface IKissCooldownAffector
    {
        Task<TimeSpan> CalculateNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown, MarriedCouple marriage,
            IList<IKissCooldownAffector> cooldownAffectors);
    }
}