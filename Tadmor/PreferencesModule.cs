using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;

namespace Tadmor
{
    public class PreferencesModule : ModuleBase<ICommandContext>
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;

        public PreferencesModule(IGuildPreferencesRepository guildPreferencesRepository)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
        }

        [Command("prefix")]
        public async Task SetPrefix(string newPrefix, PreferencesScopeCommandModel? preferencesContext)
        {
            var updatePreferencesScope = CreatePreferencesScope(preferencesContext);

            await _guildPreferencesRepository.UpdatePreferencesAsync(
                p => p.CommandPrefix = newPrefix,
                Context.Guild.Id,
                updatePreferencesScope);
        }

        private static PreferencesScope CreatePreferencesScope(PreferencesScopeCommandModel? preferencesContext)
        {
            var updatePreferencesScope = new PreferencesScope(
                preferencesContext?.Channel?.Id,
                preferencesContext?.User?.Id,
                preferencesContext?.Role?.Id);
            return updatePreferencesScope;
        }

        [NamedArgumentType]
        public class PreferencesScopeCommandModel
        {
            public ITextChannel? Channel { get; init; }
            public IGuildUser? User { get; init; }
            public IRole? Role { get; init; }
        }
    }
}