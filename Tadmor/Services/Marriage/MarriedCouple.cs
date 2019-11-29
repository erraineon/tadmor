using System;

namespace Tadmor.Services.Marriage
{
    public class MarriedCouple
    {
        public Guid Id { get; set; }
        public ulong Partner1Id { get; set; }
        public ulong Partner2Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Kisses { get; set; }
        public DateTime LastKissed { get; set; }
    }
}