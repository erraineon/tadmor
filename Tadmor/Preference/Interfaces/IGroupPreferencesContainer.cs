using System.Collections.Generic;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Interfaces
{
    public interface IGroupPreferencesContainer
    {
        Dictionary<ulong, UserPreferences> UserPreferences { get; set; }
        Dictionary<ulong, RolePreferences> RolePreferences { get; set; }
    }
}