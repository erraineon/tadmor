using Microsoft.EntityFrameworkCore;
using Tadmor.Core.Data.Interfaces;
using Tadmor.Raffles.Models;

namespace Tadmor.Raffles.Data
{
    public class RaffleWinnersModelExtension : IModelExtension
    {
        public void Extend(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RaffleExtraction>();
        }
    }
}