using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tadmor.Commands.Interfaces;

namespace Tadmor.Commands.Services
{
    public class CommandsModuleRegistrar : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandService _commandService;
        private readonly IEnumerable<IModuleRegistration> _moduleRegistrations;

        public CommandsModuleRegistrar(
            IServiceProvider serviceProvider, 
            ICommandService commandService,
            IEnumerable<IModuleRegistration> moduleRegistrations)
        {
            _serviceProvider = serviceProvider;
            _commandService = commandService;
            _moduleRegistrations = moduleRegistrations;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var moduleRegistration in _moduleRegistrations)
            {
                await _commandService.AddModuleAsync(moduleRegistration.ModuleType, _serviceProvider);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var module in _commandService.Modules)
                await _commandService.RemoveModuleAsync(module);
        }
    }
}