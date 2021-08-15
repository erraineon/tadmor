using System;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Preference.Interfaces
{
    public interface IGuildPreferencesRepository
    {
        Task<GuildPreferences?> GetGuildPreferencesAsyncOrNull(ulong guildId);

        Task UpdatePreferencesAsync(
            Action<Preferences> updateAction,
            ulong guildId,
            PreferencesScope preferencesScope);
    }
}