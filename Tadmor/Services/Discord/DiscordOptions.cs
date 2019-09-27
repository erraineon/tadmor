using System.Collections.Generic;
using System.Linq;

namespace Tadmor.Services.Discord
{
    public class DiscordOptions
    {
        public string Token { get; set; }
        public List<GuildOptions> GuildOptions { get; set; } = new List<GuildOptions>();

        public GuildOptions GetOrAddGuildOptions(ulong guildId)
        {
            var guildOptions = GuildOptions.SingleOrDefault(options => options.Id == guildId);
            if (guildOptions == null)
            {
                guildOptions = new GuildOptions { Id = guildId };
                GuildOptions.Add(guildOptions);
            }

            return guildOptions;

        }
    }
}