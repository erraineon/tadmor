using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Telegram.Interfaces;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramClientLauncher : IHostedService
    {
        private readonly ITelegramChatClient _telegramChatClient;
        private readonly ITelegramApiListener _telegramApiListener;

        public TelegramClientLauncher(ITelegramChatClient telegramChatClient, ITelegramApiListener telegramApiListener)
        {
            _telegramChatClient = telegramChatClient;
            _telegramApiListener = telegramApiListener;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _telegramApiListener.StartAsync(cancellationToken);
            await _telegramChatClient.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _telegramChatClient.StopAsync();
            await _telegramApiListener.StopAsync(cancellationToken);
        }
    }
}