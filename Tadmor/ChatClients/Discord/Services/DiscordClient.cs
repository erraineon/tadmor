using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.ChatClients.Discord.Models;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.ChatClients.Discord.Services
{
    public class DiscordClient : DiscordSocketClient, IChatClient, IHostedService
    {
        private readonly DiscordOptions _discordOptions;
        private readonly ILogger<DiscordClient> _logger;

        public DiscordClient(
            DiscordOptions discordOptions,
            ILogger<DiscordClient> logger) : base(new DiscordSocketConfig {MessageCacheSize = 100})
        {
            _discordOptions = discordOptions;
            _logger = logger;
            base.MessageReceived += message => MessageReceived(this, message);
            base.GuildMemberUpdated += (oldUser, newUser) => GuildMemberUpdated(this, oldUser, newUser);
        }

        public new event Func<IChatClient, IMessage, Task> MessageReceived = 
            (_, _) => Task.CompletedTask;

        public new event Func<IChatClient, IGuildUser, IGuildUser, Task> GuildMemberUpdated =
            (_, _, _) => Task.CompletedTask;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_discordOptions.Enabled)
            {
                Log += LogAsync;

                var discordReady = new TaskCompletionSource();

                Task OnReady()
                {
                    Ready -= OnReady;
                    discordReady.SetResult();
                    return Task.CompletedTask;
                }

                Ready += OnReady;
                await LoginAsync(TokenType.Bot, _discordOptions.Token);
                await StartAsync();
                await discordReady.Task;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_discordOptions.Enabled) await StopAsync();
        }

        private Task LogAsync(LogMessage arg)
        {
            _logger.Log(LogLevel.Information, arg.ToString());
            return Task.CompletedTask;
        }
    }
}