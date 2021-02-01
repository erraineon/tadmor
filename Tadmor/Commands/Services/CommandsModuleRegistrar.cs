using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Hosting;
using NCrontab;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.TypeReaders;

namespace Tadmor.Commands.Services
{
    public class CommandsModuleRegistrar : IHostedService
    {
        private readonly ICommandServiceScopeFactory _commandServiceScopeFactory;
        private readonly ICommandService _commandService;
        private readonly IEnumerable<IModuleRegistration> _moduleRegistrations;

        public CommandsModuleRegistrar(
            ICommandServiceScopeFactory commandServiceScopeFactory, 
            ICommandService commandService,
            IEnumerable<IModuleRegistration> moduleRegistrations)
        {
            _commandServiceScopeFactory = commandServiceScopeFactory;
            _commandService = commandService;
            _moduleRegistrations = moduleRegistrations;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var serviceScope = await _commandServiceScopeFactory.CreateScopeAsync(new InitializationCommandContext());

            if (!_commandService.TypeReaders.Contains(typeof(CrontabSchedule)))
                _commandService.AddTypeReader(typeof(CrontabSchedule), new CrontabScheduleTypeReader());

            foreach (var moduleRegistration in _moduleRegistrations)
            {
                await _commandService.AddModuleAsync(moduleRegistration.ModuleType, serviceScope.ServiceProvider);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var module in _commandService.Modules)
                await _commandService.RemoveModuleAsync(module);
        }

        private class InitializationCommandContext : ICommandContext
        {
            public IDiscordClient Client => throw new NotImplementedException();

            public IGuild Guild => throw new NotImplementedException();

            public IMessageChannel Channel => throw new NotImplementedException();

            public IUser User => throw new NotImplementedException();

            public IUserMessage Message => throw new NotImplementedException();
        }
    }
}