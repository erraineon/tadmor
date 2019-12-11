using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    public interface IBabyRankBonusAffector
    {
        Task<float> GetNewBabyRankBonus(float currentRankBonus, float rankBonus, MarriedCouple marriage, List<IBabyRankBonusAffector> babyRankBonusAffectors, ILogger logger);
    }
}