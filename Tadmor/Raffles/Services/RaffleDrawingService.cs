using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.Extensions;
using Tadmor.Raffles.Interfaces;
using Tadmor.Utilities.Modules;

namespace Tadmor.Raffles.Services
{
    public class RaffleDrawingService : IRaffleDrawingService
    {
        private readonly IRaffleWinnersRepository _raffleWinnersRepository;

        public RaffleDrawingService(IRaffleWinnersRepository raffleWinnersRepository)
        {
            _raffleWinnersRepository = raffleWinnersRepository;
        }

        public async Task<ICollection<IUser>> DrawAsync(ICollection<IUser> pool, int winnerCount, string raffleId, IRaffleBiasStrategy? raffleBiasStrategy)
        {
            var poolWithWeights = raffleBiasStrategy == null
                ? pool.Select(u => (User: u, Weight: 1f))
                : await raffleBiasStrategy.CalculateWeightsAsync(pool, raffleId);
            var winners = poolWithWeights
                .RandomSubset(winnerCount, weightFunction: t => t.Weight)
                .Select(t => t.User)
                .ToList();
            await _raffleWinnersRepository.AddWinnersAsync(winners, raffleId, DateTime.Now);
            return winners;
        }
    }
}