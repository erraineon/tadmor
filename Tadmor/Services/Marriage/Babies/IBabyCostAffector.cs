using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.Services.Marriage.Babies
{
    public interface IBabyCostAffector
    {
        Task<float> GetNewCost(float currentCost, float baseCost, MarriedCouple marriage,
            IList<IBabyCostAffector> costAffectors);
    }
}