using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Extensions;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Formatting.Interfaces;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;
using Tadmor.Core.Preference.Modules;

namespace Tadmor.Core.Commands.Modules
{
    [Summary("permissions and preferences")]
    public class CommandPreferencesModule : PreferencesModuleBase
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;
        private readonly IStringFormatter<PreferencesScope> _preferenceScopeFormatter;
        private readonly IStringFormatter<CommandPermission> _commandPermissionFormatter;

        public CommandPreferencesModule(
            IGuildPreferencesRepository guildPreferencesRepository, 
            IStringFormatter<PreferencesScope> preferenceScopeFormatter,
            IStringFormatter<CommandPermission> commandPermissionFormatter) : base(
            guildPreferencesRepository, preferenceScopeFormatter)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
            _preferenceScopeFormatter = preferenceScopeFormatter;
            _commandPermissionFormatter = commandPermissionFormatter;
        }

        [Command("prefix")]
        [Summary("sets the prefix to execute bot commands")]
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
        [Summary("shows a list of permissions")]
        public async Task<RuntimeResult> ListPermissions()
        {
            var flattenedPreferences = await GetAllScopesAndPreferencesAsync(Context.Guild.Id);
            var permissionsStrings = await Task.WhenAll(
                flattenedPreferences
                    .SelectMany(t => t.Item2.CommandPermissions, (t, perm) => (scope: t.Item1, perm))
                    .Select(
                        async (t, i) =>
                        {
                            var scope = await _preferenceScopeFormatter.ToStringAsync(t.scope);
                            var permissionInfo = await _commandPermissionFormatter.ToStringAsync(t.perm);
                            return $"{i}: {permissionInfo} {scope}";
                        }));
            return permissionsStrings.Any()
                ? CommandResult.FromSuccess(permissionsStrings)
                : CommandResult.FromError("there are no permission settings on this guild");
        }

        [Command("perms")]
        [Summary("adds the specified command to the blacklist or whitelist, optionally for a given context. using * as the command acts as a wildcard")]
        public async Task<RuntimeResult> SetPermissions(
            string command,
            CommandPermissionType permissionType,
            PreferencesScopeCommandModel? preferencesContext = default)
        {
            preferencesContext ??= new PreferencesScopeCommandModel();
            var commandPermission = new CommandPermission(command, permissionType);
            await WithPreferencesScope(
                preferencesContext,
                preferences => preferences.CommandPermissions.AddOrUpdate(commandPermission));
            var permissionInfo = await _commandPermissionFormatter.ToStringAsync(commandPermission);
            return CommandResult.FromSuccess($"{permissionInfo} for {preferencesContext}");
        }

        [Command("perms rm")]
        [Summary("removes the permission at the specified index")]
        public async Task<RuntimeResult> RemovePermissions(params int[] permissionIndexes)
        {
            var removedPermissions = await RemovePreferencesAsync(p => p.Rules, permissionIndexes);
            return CommandResult.FromSuccess($"removed {removedPermissions} permissions");
        }
    }
}