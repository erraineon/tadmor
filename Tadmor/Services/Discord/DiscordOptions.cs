using System.Collections.Generic;

namespace Tadmor.Services.Discord
{
    public class DiscordOptions
    {
        public string Token { get; set; }
        public List<GuildOptions> GuildOptions { get; set; } = new List<GuildOptions>();
    }
}