using System.ComponentModel.DataAnnotations.Schema;

namespace Tadmor.Services.Discord
{
    public class GuildOptions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        public string CommandPrefix { get; set; }
        public string WelcomeMessage { get; set; }
    }
}