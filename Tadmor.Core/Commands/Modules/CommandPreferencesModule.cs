using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Extensions;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;
using Tadmor.Core.Preference.Modules;

namespace Tadmor.Core.Commands.Modules
{
    public class CommandPreferencesModule : PreferencesModuleBase
    {
        public CommandPreferencesModule(IGuildPreferencesRepository guildPreferencesRepository) : base(
            guildPreferencesRepository)
        {
        }

        [Command("prefix")]
        public async Task<RuntimeResult> SetPrefix(
            string newPrefix,
            PreferencesScopeCommandModel? preferencesContext = null)
        {
            preferencesContext ??= new PreferencesScopeCommandModel();
            await WithPreferencesScope(
                preferencesContext,
                preferences => preferences.CommandPrefix = newPrefix);
            return CommandResult.FromSuccess($"prefix set to {newPrefix} for {preferencesContext}");
        }

        [Command("perms")]
        public async Task<RuntimeResult> SetPermissions(
            string command,
            CommandPermissionType permissionType,
            PreferencesScopeCommandModel? preferencesContext = null)
        {
            preferencesContext ??= new PreferencesScopeCommandModel();
            await WithPreferencesScope(
                preferencesContext,
                preferences => preferences.CommandPermissions
                    .AddOrUpdate(new CommandPermission(command, permissionType)));
            return CommandResult.FromSuccess(
                $"permission for command {command} set to {permissionType} for {preferencesContext}");
        }

        [Command("perms rm")]
        public async Task<RuntimeResult> RemovePermissions(
            string command,
            PreferencesScopeCommandModel? preferencesContext = null)
        {
            preferencesContext ??= new PreferencesScopeCommandModel();
            await WithPreferencesScope(
                preferencesContext,
                preferences => preferences.CommandPermissions
                    .RemoveAll(cp => cp.CommandName == command));
            return CommandResult.FromSuccess($"permission for command {command} removed for {preferencesContext}");
        }
    }
}