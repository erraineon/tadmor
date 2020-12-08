using Tadmor.Preference.Models;

namespace Tadmor.Data.Models
{
    public class GuildPreferencesEntity
    {
        public ulong GuildId { get; set; }
        public GuildPreferences Preferences { get; set; } = new();
    }
}