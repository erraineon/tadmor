using System;
using System.Threading.Tasks;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Interfaces
{
    public interface IGuildPreferencesRepository
    {
        Task<GuildPreferences?> GetGuildPreferencesAsyncOrNull(ulong guildId);

        Task UpdatePreferencesAsync(Action<Preferences> updateAction,
            ulong guildId,
            PreferencesScope preferencesScope);
    }
}