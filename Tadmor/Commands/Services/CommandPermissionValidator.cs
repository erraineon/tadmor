using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Tadmor.Commands.Attributes;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;
using Tadmor.Preference.Interfaces;

namespace Tadmor.Commands.Services
{
    public class CommandPermissionValidator : ICommandPermissionValidator
    {
        private readonly ICommandService _commandService;
        private readonly IContextualPreferencesProvider _contextualPreferencesProvider;

        public CommandPermissionValidator(
            IContextualPreferencesProvider contextualPreferencesProvider,
            ICommandService commandService)
        {
            _contextualPreferencesProvider = contextualPreferencesProvider;
            _commandService = commandService;
        }

        public async Task<bool> CanRunAsync(
            ExecuteCommandRequest executeCommandRequest,
            CancellationToken cancellationToken)
        {
            var (commandContext, input) = executeCommandRequest;
            var preferences = await _contextualPreferencesProvider.GetContextualPreferences(
                (IGuildChannel) commandContext.Channel,
                (IGuildUser) commandContext.User);
            bool canRun;
            if (preferences.CommandPermissions.SingleOrDefault(cp => cp.CommandName == "*") is { } wildcardPermission)
            {
                canRun = wildcardPermission.CommandPermissionType == CommandPermissionType.Whitelist;
            }
            else if (_commandService.Search(commandContext, input) is {IsSuccess: true} searchResult)
            {
                var commandMatch = searchResult.Commands.First();
                var commandName = commandMatch.Command.Name ?? commandMatch.Alias;
                var permission = preferences.CommandPermissions
                    .SingleOrDefault(cp => string.Equals(cp.CommandName, commandName, StringComparison.Ordinal));
                var requiresWhitelist = commandMatch.Command.Attributes.OfType<RequireWhitelistAttribute>().Any();
                canRun = requiresWhitelist && permission is {CommandPermissionType: CommandPermissionType.Whitelist} ||
                    !requiresWhitelist && permission is not {CommandPermissionType: CommandPermissionType.Blacklist};
            }
            else
            {
                canRun = true;
            }

            return canRun;
        }
    }
}