using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Telegram.Interfaces;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramCommandMetadataRegistrar : IHostedService
    {
        private readonly ITelegramApiClient _telegramClient;

        public TelegramCommandMetadataRegistrar(ITelegramApiClient telegramClient)
        {
            _telegramClient = telegramClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: call setMyCommands, be aware of commands that need special permissions
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}