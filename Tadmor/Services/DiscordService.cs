using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tadmor.Services
{
    internal class DiscordService
    {
        private static readonly LogLevel[] LogLevels =
        {
            LogLevel.Critical, LogLevel.Error, LogLevel.Warning,
            LogLevel.Information, LogLevel.Trace, LogLevel.Debug
        };

        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly DiscordOptions _discordOptions;
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public DiscordService(IServiceProvider services, ILogger<DiscordService> logger, DiscordSocketClient discord,
            CommandService commands,
            IOptions<DiscordOptions> discordOptions)
        {
            _services = services;
            _logger = logger;
            _discord = discord;
            _commands = commands;
            _discordOptions = discordOptions.Value;
        }

        public async Task Start()
        {
            _discord.Log += Log;
            _commands.Log += OnCommandError;
            _discord.MessageReceived += TryExecuteCommand;
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly());
            await _discord.LoginAsync(TokenType.Bot, _discordOptions.Token);
            await _discord.StartAsync();
        }

        private Task Log(LogMessage logMessage)
        {
            //discord log severity and log levels don't follow the same order so map them
            var logLevel = LogLevels[(int) logMessage.Severity];
            _logger.Log(logLevel, 0, logMessage, logMessage.Exception, (m, e) => m.ToString());
            return Task.CompletedTask;
        }

        private async Task OnCommandError(LogMessage logMessage)
        {
            if (logMessage.Exception is CommandException e)
                await e.Context.Channel.SendMessageAsync(e.InnerException.Message);
        }

        private async Task TryExecuteCommand(SocketMessage socketMessage)
        {
            const string commandPrefix = ".";
            if (!(socketMessage.Channel is IPrivateChannel) &&
                socketMessage is SocketUserMessage message &&
                !message.Author.IsBot &&
                message.Content.StartsWith(commandPrefix))
            {
                var context = new SocketCommandContext(_discord, message);
                var result = await _commands.ExecuteAsync(context, commandPrefix.Length, _services);
            }
        }
    }
}