using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tadmor.Services.Commands;

namespace Tadmor.Services.Discord
{
    public class DiscordService : IHostedService
    {
        private static readonly LogLevel[] LogLevels =
        {
            LogLevel.Critical, LogLevel.Error, LogLevel.Warning,
            LogLevel.Information, LogLevel.Trace, LogLevel.Debug
        };

        private readonly DiscordSocketClient _discord;
        private readonly ChatCommandsService _commands;
        private readonly DiscordOptions _discordOptions;
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public DiscordService(
            IServiceProvider services,
            ILogger<DiscordService> logger,
            DiscordSocketClient discord,
            IOptions<DiscordOptions> discordOptions,
            ChatCommandsService commands)
        {
            _services = services;
            _logger = logger;
            _discord = discord;
            _commands = commands;
            _discordOptions = discordOptions.Value;
        }

        private Task Log(LogMessage logMessage)
        {
            //discord log severity and log levels don't follow the same order so map them
            var logLevel = LogLevels[(int) logMessage.Severity];
            _logger.Log(logLevel, 0, logMessage, logMessage.Exception, (m, e) => m.ToString());
            return Task.CompletedTask;
        }

        private async Task TryExecuteCommand(SocketMessage socketMessage)
        {
            if (socketMessage.Channel is IGuildChannel channel &&
                socketMessage is SocketUserMessage message &&
                !message.Author.IsBot &&
                GetCommandsPrefix(channel.Guild) is var commandPrefix &&
                message.Content.StartsWith(commandPrefix))
            {
                var context = new SocketCommandContext(_discord, message);
                await _commands.ExecuteCommand(context, commandPrefix);
            }
        }

        public string GetCommandsPrefix(IGuild guild)
        {
            using var scope = _services.CreateScope();
            var discordOptions = scope.ServiceProvider.GetService<IOptionsSnapshot<DiscordOptions>>().Value;
            var guildOptions = discordOptions.GuildOptions.SingleOrDefault(options => options.Id == guild.Id);
            var commandPrefix = guildOptions?.CommandPrefix is var p && !string.IsNullOrEmpty(p) ? p : ".";
            return commandPrefix;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var discordReady = new TaskCompletionSource<object>();

            Task OnReady()
            {
                _discord.Ready -= OnReady;
                discordReady.SetResult(default);
                return Task.CompletedTask;
            }

            _discord.Ready += OnReady;
            _discord.Log += Log;
            _discord.MessageReceived += TryExecuteCommand;
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