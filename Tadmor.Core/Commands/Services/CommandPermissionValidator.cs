using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Preference.Interfaces;

namespace Tadmor.Core.Commands.Services
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

        public async Task<bool?> CanRunAsync(
            ExecuteCommandRequest executeCommandRequest,
            CancellationToken cancellationToken)
        {
            var (commandContext, input) = executeCommandRequest;
            return _commandService.Search(commandContext, input) is {IsSuccess: true} searchResult
                ? (bool?) await CanRunAsync(commandContext, searchResult.Commands.First().Command)
                : null;
        }

        public async Task<bool> CanRunAsync(ICommandContext commandContext, CommandInfo commandInfo)
        {
            var preferences = await _contextualPreferencesProvider.GetContextualPreferencesAsync(
                (IGuildChannel) commandContext.Channel,
                (IGuildUser) commandContext.User);
            bool canRun;
            if (preferences.CommandPermissions.SingleOrDefault(cp => cp.CommandName == "*") is { } wildcardPermission)
            {
                canRun = wildcardPermission.CommandPermissionType == CommandPermissionType.Whitelist;
            }
            else
            {
                var commandName = commandInfo.Name ?? commandInfo.Aliases.First();
                var permission = preferences.CommandPermissions
                    .SingleOrDefault(cp => string.Equals(cp.CommandName, commandName, StringComparison.Ordinal));
                var requiresWhitelist = commandInfo.Attributes.OfType<RequireWhitelistAttribute>().Any();
                canRun = requiresWhitelist && permission is {CommandPermissionType: CommandPermissionType.Whitelist} ||
                         !requiresWhitelist && permission is not {CommandPermissionType: CommandPermissionType.Blacklist};
            }

            return canRun;
        }
    }
}