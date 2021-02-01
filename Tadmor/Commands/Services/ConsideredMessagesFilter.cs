using System.Threading;
using System.Threading.Tasks;
using Discord;
using Tadmor.Abstractions.Models;
using Tadmor.Commands.Models;
using Tadmor.Notifications.Interfaces;

namespace Tadmor.Commands.Services
{
    public class ConsideredMessagesFilter : INotificationHandler<MessageReceivedNotification>
    {
        private readonly INotificationPublisher _notificationPublisher;

        public ConsideredMessagesFilter(INotificationPublisher notificationPublisher)
        {
            _notificationPublisher = notificationPublisher;
        }
        
        public async Task HandleAsync(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            var (chatClient, message) = notification;
            if (message is IUserMessage userMessage &&
                message.Channel is IGuildChannel guildChannel &&
                message.Author is IGuildUser { IsBot: false } guildUser)
            {
                var messageValidatedNotification = new MessageValidatedNotification(chatClient, userMessage, guildChannel, guildUser);
                await _notificationPublisher.PublishAsync(messageValidatedNotification, cancellationToken);
            }
        }
    }
}