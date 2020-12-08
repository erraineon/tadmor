using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Commands.Interfaces;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Models;
using Tadmor.Preference.Interfaces;

namespace Tadmor.Commands.Services
{
    public class CommandListener : INotificationHandler<MessageReceivedNotification>
    {
        private readonly IContextualPreferencesProvider _contextualPreferencesProvider;
        private readonly ICommandExecutor _commandExecutor;

        public CommandListener(
            IContextualPreferencesProvider contextualPreferencesProvider, 
            ICommandExecutor commandExecutor)
        {
            _contextualPreferencesProvider = contextualPreferencesProvider;
            _commandExecutor = commandExecutor;
        }

        public async Task HandleAsync(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            var (chatClient, message) = notification;
            if (message is IUserMessage userMessage &&
                message.Channel is IGuildChannel guildChannel &&
                message.Author is IGuildUser guildUser)
            {
                var context = new CommandContext(chatClient, userMessage);
                var preferences = await _contextualPreferencesProvider.GetContextualPreferences(guildChannel, guildUser);
                if (userMessage.Content.StartsWith(preferences.CommandPrefix))
                {
                    var input = userMessage.Content.Substring(preferences.CommandPrefix.Length);
                    await _commandExecutor.BeginExecutionAsync(context, input);
                }
            }
        }
    }
}