using System;

namespace Tadmor.Raffles.Models
{
    public class RaffleStatistics
    {
        public ulong UserId { get; set; }
        public string Username { get; set; } = null!;
        public DateTime LastWinDate { get; set; }
        public int TotalWins { get; set; }
    }
}