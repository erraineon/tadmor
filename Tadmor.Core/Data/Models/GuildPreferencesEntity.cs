using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Data.Models
{
    public class GuildPreferencesEntity
    {
        public ulong GuildId { get; set; }
        public GuildPreferences Preferences { get; set; } = new();
    }
}