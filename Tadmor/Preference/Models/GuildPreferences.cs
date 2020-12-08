using System.Collections.Generic;

namespace Tadmor.Preference.Models
{
    public class GuildPreferences : GroupPreferencesContainer
    {
        public Dictionary<ulong, ChannelPreferences> ChannelPreferences { get; set; } = new();
    }
}