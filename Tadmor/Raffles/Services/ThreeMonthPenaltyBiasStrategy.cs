using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using MoreLinq.Extensions;
using Tadmor.Raffles.Interfaces;

namespace Tadmor.Raffles.Services
{
    public class ThreeMonthPenaltyBiasStrategy : IRaffleBiasStrategy
    {
        private readonly IRaffleWinnersRepository _raffleWinnersRepository;

        public ThreeMonthPenaltyBiasStrategy(IRaffleWinnersRepository raffleWinnersRepository)
        {
            _raffleWinnersRepository = raffleWinnersRepository;
        }

        public async Task<IEnumerable<(IUser User, float Weight)>> CalculateWeightsAsync(
            ICollection<IUser> pool, string raffleId)
        {
            var statistics = await _raffleWinnersRepository.GetStatisticsAsync(raffleId);
            var usersAndWeights = pool.LeftJoin(statistics, u => u.Id, s => s.UserId,
                // if no statistics are found, regular weight
                u => (u, 1f),
                (u, s) =>
                {
                    var hasRecentlyExtractedPenalty = DateTime.Now - s.LastWinDate < TimeSpan.FromDays(90);
                    return (u, hasRecentlyExtractedPenalty ? 0.5f : 1);
                });
            return usersAndWeights;
        }

        public string Name => "everyone";
    }
}