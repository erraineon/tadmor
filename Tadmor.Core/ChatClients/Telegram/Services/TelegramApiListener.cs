using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Telegram.Bot.Types;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramApiListener : ITelegramApiListener
    {
        private readonly ITelegramApiClient _api;
        private readonly ILogger<TelegramApiListener> _logger;
        private readonly ITelegramEventProvider _telegramEventProvider;

        public TelegramApiListener(
            ITelegramApiClient api,
            ITelegramEventProvider telegramEventProvider,
            ILogger<TelegramApiListener> logger)
        {
            _api = api;
            _telegramEventProvider = telegramEventProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _api.MessageReceivedAsync += OnApiMessageReceived;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _api.MessageReceivedAsync -= OnApiMessageReceived;
            return Task.CompletedTask;
        }

        private async Task OnApiMessageReceived(Message message)
        {
            try
            {
                await _telegramEventProvider.HandleInboundMessageAsync(message);
            }
            catch (Exception exception)
            {
                // todo: replace .Text access with message formatter
                _logger.LogError(exception, $"error when receiving telegram message: {message.Text}");
            }
        }
    }
}