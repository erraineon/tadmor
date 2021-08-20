using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.Raffles.Interfaces
{
    public interface IRaffleDrawingService
    {
        Task<ICollection<IUser>> DrawAsync(ICollection<IUser> pool, int winnerCount, string raffleId, IRaffleBiasStrategy? raffleBiasStrategy);
    }
}