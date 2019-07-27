using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tadmor.Services.Discord
{
    public class GuildOptions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        public string CommandPrefix { get; set; }
        public string WelcomeMessage { get; set; }
        public ulong WelcomeChannel { get; set; }
        public List<GuildEvent> Events { get; set; } = new List<GuildEvent>();
    }
}