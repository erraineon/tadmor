using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Tadmor.Raffles.Models;

namespace Tadmor.Raffles.Interfaces
{
    public interface IRaffleWinnersRepository
    {
        Task<ICollection<RaffleStatistics>> GetStatisticsAsync(string raffleId);
        Task AddWinnersAsync(ICollection<IUser> winners, string raffleId, DateTime extractionTime);
        Task<IList<RaffleExtraction>> GetAllExtractionsAsync(string raffleId);
    }
}