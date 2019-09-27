using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Extensions;

namespace Tadmor.Services.Commands
{
    public class ChatCommandsService : IHostedService
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public ChatCommandsService(IServiceProvider services)
        {
            _services = services;
            _commands = new CommandService(new CommandServiceConfig { DefaultRunMode = RunMode.Async });
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
            if (logMessage.Exception is CommandException commandException)
            {
                var e = commandException.InnerException;
                var message = e.GetType() == typeof(Exception) ? e.Message : e.ToShortString();
                await commandException.Context.Channel.SendMessageAsync(message);
            }
        }

        public async Task ExecuteCommand(ICommandContext context, string prefix)
        {
            var scope = _services.CreateScope();
            var result = await _commands.ExecuteAsync(context, prefix.Length, scope.ServiceProvider);

            // as of DependencyInjection v2.1 scope disposal is immediate whereas precondition check is asynchronous
            // therefore scope disposal must be made asynchronous too
            _commands.CommandExecuted += DisposeScope;

            Task DisposeScope(Optional<CommandInfo> _, ICommandContext completedContext, IResult __)
            {
                if (completedContext == context)
                {
                    scope.Dispose();
                    _commands.CommandExecuted -= DisposeScope;
                }

                return Task.CompletedTask;
            }

            if (result.Error == CommandError.UnmetPrecondition) await context.Channel.SendMessageAsync("no");
        }

        public IEnumerable<IGrouping<ModuleInfo, CommandInfo>> GetCommandsByRoot()
        {
            ModuleInfo Root(ModuleInfo module) => module.Parent == null ? module : Root(module.Parent);
            return _commands.Commands
                .Where(c => c.Attributes.OfType<BrowsableAttribute>().All(a => a.Browsable))
                .GroupBy(command => Root(command.Module));
        }
    }
}