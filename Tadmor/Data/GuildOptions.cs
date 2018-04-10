using System.ComponentModel.DataAnnotations.Schema;

namespace Tadmor.Data
{
    public class GuildOptions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        public string CommandPrefix { get; set; }
    }
}