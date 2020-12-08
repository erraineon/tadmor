using System;
using System.Threading.Tasks;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Interfaces
{
    public interface IGuildPreferencesRepository
    {
        Task<GuildPreferences?> GetGuildPreferences(ulong guildId);

        Task UpdatePreferences(Action<Preferences> updateAction,
            ulong guildId,
            PreferencesScope preferencesScope);
    }
}