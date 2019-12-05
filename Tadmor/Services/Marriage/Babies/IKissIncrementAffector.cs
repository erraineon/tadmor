using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Marriage.Babies
{
    public interface IKissIncrementAffector
    {
        Task<float> GetNewIncrement(float currentIncrement, float baseKissIncrement, MarriedCouple marriage,
            IList<IKissIncrementAffector> kissAffectors, ILogger logger);
    }
}