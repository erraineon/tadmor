using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tadmor.Data;
using Tadmor.Extensions;

namespace Tadmor.Services.Discord
{
    public class DiscordService
    {
        private static readonly LogLevel[] LogLevels =
        {
            LogLevel.Critical, LogLevel.Error, LogLevel.Warning,
            LogLevel.Information, LogLevel.Trace, LogLevel.Debug
        };

        private readonly CommandService _commands;
        private readonly AppDbContext _dbContext;
        private readonly DiscordSocketClient _discord;
        private readonly DiscordOptions _discordOptions;
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private Dictionary<ulong, GuildOptions> _guildOptions;

        public DiscordService(IServiceProvider services, ILogger<DiscordService> logger, DiscordSocketClient discord,
            CommandService commands, IOptions<DiscordOptions> discordOptions, AppDbContext dbContext)
        {
            _services = services;
            _logger = logger;
            _discord = discord;
            _commands = commands;
            _dbContext = dbContext;
            _discordOptions = discordOptions.Value;
        }

        public async Task Start()
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
            _commands.Log += LogCommandError;
            _discord.MessageReceived += TryExecuteCommand;
            await ReloadGuildOptions();
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly());
            await _discord.LoginAsync(TokenType.Bot, _discordOptions.Token);
            await _discord.StartAsync();
            await discordReady.Task;
        }

        private async Task ReloadGuildOptions()
        {
            //to avoid continously querying the database, load guild settings on demand
            _guildOptions = await _dbContext.GuildOptions.ToDictionaryAsync(o => o.Id);
        }
        
        private Task Log(LogMessage logMessage)
        {
            //discord log severity and log levels don't follow the same order so map them
            var logLevel = LogLevels[(int) logMessage.Severity];
            _logger.Log(logLevel, 0, logMessage, logMessage.Exception, (m, e) => m.ToString());
            return Task.CompletedTask;
        }

        private async Task LogCommandError(LogMessage logMessage)
        {
            if (logMessage.Exception is CommandException commandException)
            {
                var e = commandException.InnerException;
                var message = e.GetType() == typeof(Exception) ? e.Message : e.ToShortString();
                await commandException.Context.Channel.SendMessageAsync(message);
            }
        }

        private async Task TryExecuteCommand(SocketMessage socketMessage)
        {
            if (socketMessage.Channel is IGuildChannel channel &&
                socketMessage is SocketUserMessage message &&
                !message.Author.IsBot)
            {
                var commandPrefix = _guildOptions.TryGetValue(channel.GuildId, out var guildOptions)
                    ? guildOptions.CommandPrefix
                    : ".";
                if (message.Content.StartsWith(commandPrefix))
                {
                    var context = new SocketCommandContext(_discord, message);
                    var result = await _commands.ExecuteAsync(context, commandPrefix.Length, _services);
                }
            }
        }

        public async Task ChangeCommandPrefix(SocketGuild guild, string newPrefix)
        {
            var key = guild.Id;
            var options = await _dbContext.GuildOptions.FindAsync(key);
            if (options == null)
            {
                options = new GuildOptions {Id = key};
                await _dbContext.GuildOptions.AddAsync(options);
            }

            options.CommandPrefix = newPrefix;
            await _dbContext.SaveChangesAsync();
            await ReloadGuildOptions();
        }
    }
}