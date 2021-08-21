using System.Collections.Generic;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Preference.Interfaces
{
    public interface IGroupPreferencesContainer
    {
        Dictionary<ulong, UserPreferences> UserPreferences { get; set; }
        Dictionary<ulong, RolePreferences> RolePreferences { get; set; }
    }
}