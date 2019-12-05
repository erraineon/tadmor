using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    public interface IKissCooldownAffector
    {
        Task<TimeSpan> GetNewCooldown(TimeSpan currentCooldown, TimeSpan baseCooldown, MarriedCouple marriage,
            IList<IKissCooldownAffector> cooldownAffectors, ILogger logger);
    }
}