using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Preference.Modules
{
    public abstract class PreferencesModuleBase : ModuleBase<ICommandContext>
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;

        protected PreferencesModuleBase(IGuildPreferencesRepository guildPreferencesRepository)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
        }

        protected virtual async Task WithPreferencesScope(
            PreferencesScopeCommandModel preferencesContext,
            Action<Preferences> updateAction)
        {
            var updatePreferencesScope = new PreferencesScope(
                preferencesContext.Channel?.Id,
                preferencesContext.User?.Id,
                preferencesContext.Role?.Id);
            await _guildPreferencesRepository.UpdatePreferencesAsync(
                updateAction,
                Context.Guild.Id,
                updatePreferencesScope);
        }
    }
}