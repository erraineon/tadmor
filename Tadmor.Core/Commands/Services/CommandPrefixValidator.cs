﻿using System.Threading;
using System.Threading.Tasks;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Preference.Interfaces;

namespace Tadmor.Core.Commands.Services
{
    public class CommandPrefixValidator : ICommandPrefixValidator
    {
        private readonly IContextualPreferencesProvider _contextualPreferencesProvider;

        public CommandPrefixValidator(IContextualPreferencesProvider contextualPreferencesProvider)
        {
            _contextualPreferencesProvider = contextualPreferencesProvider;
        }

        public async Task<CommandPrefixValidationResult> ValidatePrefix(
            MessageValidatedNotification notification,
            CancellationToken cancellationToken)
        {
            var (_, userMessage, guildChannel, guildUser) = notification;
            var preferences = await _contextualPreferencesProvider.GetContextualPreferencesAsync(guildChannel, guildUser);
            var prefix = preferences.CommandPrefix ?? ".";
            var isPrefixValid = userMessage.Content.StartsWith(prefix);
            var input = isPrefixValid
                ? userMessage.Content[prefix.Length..]
                : string.Empty;
            var result = new CommandPrefixValidationResult(isPrefixValid, input);
            return result;
        }
    }
}