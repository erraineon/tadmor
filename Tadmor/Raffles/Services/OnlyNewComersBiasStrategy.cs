using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using MoreLinq.Extensions;
using Tadmor.Raffles.Interfaces;

namespace Tadmor.Raffles.Services
{
    public class OnlyNewComersBiasStrategy : IRaffleBiasStrategy
    {
        private readonly IRaffleWinnersRepository _raffleWinnersRepository;

        public OnlyNewComersBiasStrategy(IRaffleWinnersRepository raffleWinnersRepository)
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
                // if they are found, zero weight
                (u, _) => (u, 0f));
            return usersAndWeights;
        }

        public string Name => "newcomers";
    }
}