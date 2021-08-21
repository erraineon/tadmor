using System.Collections.Generic;
using Tadmor.Core.Preference.Interfaces;

namespace Tadmor.Core.Preference.Models
{
    public abstract class GroupPreferencesContainer : Preferences, IGroupPreferencesContainer
    {
        public Dictionary<ulong, UserPreferences> UserPreferences { get; set; } = new();
        public Dictionary<ulong, RolePreferences> RolePreferences { get; set; } = new();
    }
}