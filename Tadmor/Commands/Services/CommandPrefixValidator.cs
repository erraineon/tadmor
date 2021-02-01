using System.Threading;
using System.Threading.Tasks;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;
using Tadmor.Preference.Interfaces;

namespace Tadmor.Commands.Services
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
            var preferences = await _contextualPreferencesProvider.GetContextualPreferences(guildChannel, guildUser);
            var isPrefixValid = userMessage.Content.StartsWith(preferences.CommandPrefix);
            var input = isPrefixValid
                ? userMessage.Content[preferences.CommandPrefix.Length..]
                : string.Empty;
            var result = new CommandPrefixValidationResult(isPrefixValid, input);
            return result;
        }
    }
}