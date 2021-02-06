using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tadmor.ChatClients.Telegram.Interfaces;

namespace Tadmor.ChatClients.Telegram.Services
{
    public class TelegramClientLauncher : IHostedService
    {
        private readonly ITelegramClient _telegramClient;
        private readonly ITelegramApiListener _telegramApiListener;

        public TelegramClientLauncher(ITelegramClient telegramClient, ITelegramApiListener telegramApiListener)
        {
            _telegramClient = telegramClient;
            _telegramApiListener = telegramApiListener;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _telegramApiListener.StartAsync(cancellationToken);
            await _telegramClient.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _telegramClient.StopAsync();
            await _telegramApiListener.StopAsync(cancellationToken);
        }
    }
}