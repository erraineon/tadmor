using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Tadmor.Services.Discord
{
    [TransientService]
    public class DiscordService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly DiscordOptions _discordOptions;

        public DiscordService(
            DiscordSocketClient discord,
            IOptions<DiscordOptions> discordOptions)
        {
            _discord = discord;
            _discordOptions = discordOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var discordReady = new TaskCompletionSource<object?>();

            Task OnReady()
            {
                _discord.Ready -= OnReady;
                discordReady.SetResult(default);
                return Task.CompletedTask;
            }

            _discord.Ready += OnReady;
            await _discord.LoginAsync(TokenType.Bot, _discordOptions.Token);
            await _discord.StartAsync();
            await discordReady.Task;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discord.Dispose();
            return Task.CompletedTask;
        }
    }
}