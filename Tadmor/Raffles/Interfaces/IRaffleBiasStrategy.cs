using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.Raffles.Interfaces
{
    public interface IRaffleBiasStrategy
    {
        Task<IEnumerable<(IUser User, float Weight)>> CalculateWeightsAsync(ICollection<IUser> pool,
            string raffleId);

        string Name { get; }
    }
}