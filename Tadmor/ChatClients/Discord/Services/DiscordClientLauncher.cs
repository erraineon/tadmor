using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Hosting;
using Tadmor.ChatClients.Discord.Interfaces;
using Tadmor.ChatClients.Discord.Models;

namespace Tadmor.ChatClients.Discord.Services
{
    public class DiscordClientLauncher : IHostedService
    {
        private readonly IDiscordChatClient _discordChatClient;
        private readonly DiscordOptions _discordOptions;

        public DiscordClientLauncher(
            IDiscordChatClient discordChatClient,
            DiscordOptions discordOptions)
        {
            _discordChatClient = discordChatClient;
            _discordOptions = discordOptions;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var discordReady = new TaskCompletionSource();

            Task OnReady()
            {
                _discordChatClient.Ready -= OnReady;
                discordReady.SetResult();
                return Task.CompletedTask;
            }

            _discordChatClient.Ready += OnReady;
            await _discordChatClient.LoginAsync(TokenType.Bot, _discordOptions.Token, true);
            await _discordChatClient.StartAsync();
            await discordReady.Task;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordChatClient.StopAsync();
        }
    }
}