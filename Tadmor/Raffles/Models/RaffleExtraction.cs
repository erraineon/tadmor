using System;

namespace Tadmor.Raffles.Models
{
    public class RaffleExtraction
    {
        public int Id { get; set; }
        public DateTime ExtractionTime { get; set; }
        public ulong UserId { get; set; }
        public string Username { get; set; } = null!;
        public string RaffleId { get; set; } = null!;
    }
}