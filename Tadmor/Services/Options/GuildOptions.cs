using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Tadmor.Services.Discord;

namespace Tadmor.Services.Options
{
    [Options]
    public class GuildOptions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        public string? CommandPrefix { get; set; }
        public List<GuildEvent> Events { get; set; } = new List<GuildEvent>();
        public bool GoodBoyMode { get; set; }

    }
}