using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Tadmor.Core.Data.Interfaces;
using Tadmor.Raffles.Interfaces;
using Tadmor.Raffles.Models;

namespace Tadmor.Raffles.Services
{
    public class RaffleWinnersRepository : IRaffleWinnersRepository
    {
        private readonly ITadmorDbContext _tadmorDbContext;

        public RaffleWinnersRepository(ITadmorDbContext tadmorDbContext)
        {
            _tadmorDbContext = tadmorDbContext;
        }

        public async Task<ICollection<RaffleStatistics>> GetStatisticsAsync(string raffleId)
        {
            var statistics = await _tadmorDbContext.Set<RaffleExtraction>().AsQueryable()
                .Where(r => r.RaffleId == raffleId)
                .GroupBy(r => new { r.UserId, r.Username })
                .Select(g => new RaffleStatistics
                {
                    UserId = g.Key.UserId,
                    Username = g.Key.Username,
                    LastWinDate = g.Max(r => r.ExtractionTime),
                    TotalWins = g.Count()
                })
                .ToListAsync();
            return statistics;
        }

        public async Task AddWinnersAsync(ICollection<IUser> winners, string raffleId, DateTime extractionTime)
        {
            var raffleExtractions = winners.Select(w => new RaffleExtraction
            {
                ExtractionTime = extractionTime,
                RaffleId = raffleId,
                Username = w.Username,
                UserId = w.Id
            });
            await _tadmorDbContext.Set<RaffleExtraction>().AddRangeAsync(raffleExtractions);
            await _tadmorDbContext.SaveChangesAsync();
        }

        public async Task<IList<RaffleExtraction>> GetAllExtractionsAsync(string raffleId)
        {
            return await _tadmorDbContext.Set<RaffleExtraction>().AsQueryable()
                .Where(r => r.RaffleId == raffleId)
                .ToListAsync();
        }
    }
}