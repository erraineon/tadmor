using System.Collections.Generic;
using Tadmor.Preference.Interfaces;

namespace Tadmor.Preference.Models
{
    public abstract class GroupPreferencesContainer : Preferences, IGroupPreferencesContainer
    {
        public Dictionary<ulong, UserPreferences> UserPreferences { get; set; } = new();
        public Dictionary<ulong, RolePreferences> RolePreferences { get; set; } = new();
    }
}