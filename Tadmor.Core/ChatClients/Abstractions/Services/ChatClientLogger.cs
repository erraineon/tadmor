using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tadmor.Core.ChatClients.Abstractions.Models;
using Tadmor.Core.Notifications.Interfaces;

namespace Tadmor.Core.ChatClients.Abstractions.Services
{
    public class ChatClientLogger : INotificationHandler<LogNotification>, INotificationHandler<MessageReceivedNotification>
    {
        private readonly ILogger<ChatClientLogger> _logger;

        public ChatClientLogger(ILogger<ChatClientLogger> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(LogNotification notification, CancellationToken cancellationToken)
        {
            _logger.Log(LogLevel.Information, notification.LogMessage.ToString());
            return Task.CompletedTask;
        }

        public Task HandleAsync(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            _logger.Log(LogLevel.Information, notification.Message.ToString());
            return Task.CompletedTask;
        }
    }
}