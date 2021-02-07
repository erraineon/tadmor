using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tadmor.ChatClients.Abstractions.Models;
using Tadmor.Notifications.Interfaces;

namespace Tadmor.ChatClients.Abstractions.Services
{
    public class ChatClientLogger : INotificationHandler<LogNotification>
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
    }
}