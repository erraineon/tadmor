using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Tadmor.Services.Marriage.Babies;

namespace Tadmor.Services.Marriage
{
    public class MarriedCouple
    {
        public Guid Id { get; set; }
        public ulong Partner1Id { get; set; }
        public ulong Partner2Id { get; set; }
        public DateTime TimeStamp { get; set; }
        [Column("FloatKisses")]
        public float Kisses { get; set; }
        [Column("Kisses")]
        [Obsolete]
        public int KissesLegacy { get; set; }
        public DateTime LastKissed { get; set; }
        public ulong GuildId { get; set; }
        public IList<Baby> Babies { get; set; }
        public TimeSpan KissCooldown { get; set; }
    }
}