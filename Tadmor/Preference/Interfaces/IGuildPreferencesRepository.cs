using System;
using System.Threading.Tasks;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Interfaces
{
    public interface IGuildPreferencesRepository
    {
        Task<GuildPreferences?> GetGuildPreferencesAsync(ulong guildId);

        Task UpdatePreferencesAsync(Action<Preferences> updateAction,
            ulong guildId,
            PreferencesScope preferencesScope);
    }
}