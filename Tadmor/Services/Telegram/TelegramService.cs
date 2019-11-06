using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tadmor.Adapters.Telegram;
using Tadmor.Services.Abstractions;

namespace Tadmor.Services.Telegram
{
    [SingletonService]
    public class TelegramService : IHostedService
    {
        private readonly TelegramClient _client;
        private readonly TelegramOptions _options;

        public TelegramService(IOptionsSnapshot<TelegramOptions> options, TelegramClient client)
        {
            _client = client;
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.LoginAsync(_options.Token, _options.BotOwnerId);
            await _client.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _client.StopAsync();
        }
    }
}