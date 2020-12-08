using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tadmor.Services.Commands
{
    [SingletonService]
    public class CommandsService : IHostedService
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly CommandPermissionService _commandPermissionService;

        public CommandsService(IServiceProvider services, ILogger<CommandService> logger, CommandPermissionService commandPermissionService)
        {
            _services = services;
            _logger = logger;
            _commandPermissionService = commandPermissionService;
            _commands = new CommandService(new CommandServiceConfig {DefaultRunMode = RunMode.Async});
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
            _commands.Log += LogCommandError;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _commands.Log -= LogCommandError;
            return Task.CompletedTask;
        }

        private async Task LogCommandError(LogMessage logMessage)
        {
            static string ToShortString(Exception ex)
            {
                var builder = new StringBuilder(ex.Message);
                var (path, line) = new StackTrace(ex, true).GetFrames()
                    .Select(frame => (path: frame.GetFileName(), line: frame.GetFileLineNumber()))
                    .FirstOrDefault(t => t.path != null);
                if (path != null) builder.Append($" in {Path.GetFileName(path)} at line {line}");
                return builder.ToString();
            }

            if (logMessage.Exception is CommandException commandException)
            {
                var e = commandException.InnerException ?? commandException;
                if (e.GetType() == typeof(Exception))
                {
                    await commandException.Context.Channel.SendMessageAsync(e.Message);
                }
                else
                {
                    _logger.LogError(ToShortString(e));
                    await commandException.Context.Channel.SendMessageAsync("https://i.imgur.com/5DODrAB.png");
                }
            }
        }

        public async Task ExecuteCommand(ICommandContext context, string prefix)
        {
            var commandIndex = prefix.Length;
            var commandsSearch = _commands.Search(context, commandIndex);
            if (commandsSearch.IsSuccess && 
                (!await _commandPermissionService.PassesLists(context, commandsSearch.Commands.First().Command) ||
                await ExecuteCommandNoPermissionsCheck(context, prefix) is var result &&
                result.Error == CommandError.UnmetPrecondition))
            {
                await context.Channel.SendMessageAsync("no");
            }
        }

        private async Task<IResult> ExecuteCommandNoPermissionsCheck(ICommandContext context, string prefix)
        {
            var scope = _services.CreateScope();
            var commandScope = scope.ServiceProvider.GetService<CommandContextResolver>();
            commandScope.CurrentCommandContext = context;
            var result = await _commands.ExecuteAsync(context, prefix.Length, scope.ServiceProvider);

            _commands.CommandExecuted += DisposeScope;
            Task DisposeScope(Optional<CommandInfo> _, ICommandContext completedContext, IResult __)
            {
                if (completedContext == context)
                {
                    scope.Dispose();
                    _commands.CommandExecuted -= DisposeScope;
                    _logger.LogInformation($"ok: {context.Message.Content}");
                }

                return Task.CompletedTask;
            }

            return result;
        }

        public IAsyncEnumerable<IAsyncGrouping<ModuleInfo, CommandInfo>> GetAvailableCommandsAsync(
            ICommandContext context)
        {
            return _commands.Commands
                .Where(c => c.Attributes.OfType<BrowsableAttribute>().All(a => a.Browsable))
                .ToAsyncEnumerable()
                .SelectAwait(async command =>
                {
                    var result = await command.CheckPreconditionsAsync(context, _services);
                    return (command, result.IsSuccess);
                })
                .Where(t => t.IsSuccess)
                .GroupBy(t =>
                {
                    var module = t.command.Module;
                    while (module.Parent != null) module = module.Parent;
                    return module;
                }, t => t.command);
        }

        public async Task<bool> IsCommandAvailableAsync(ICommandContext context, string commandName)
        {
            return await _commands.Commands
                .ToAsyncEnumerable()
                .AnyAwaitAsync(async c => string.Equals(c.Name, commandName, StringComparison.OrdinalIgnoreCase) &&
                                          (await c.CheckPreconditionsAsync(context, _services)).IsSuccess);
        }
    }
}