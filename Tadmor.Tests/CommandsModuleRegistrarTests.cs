using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class CommandsModuleRegistrarTests
    {
        private CommandsModuleRegistrar _sut;
        private ICommandService _commandService;
        private IModuleRegistration _moduleRegistration1;
        private IModuleRegistration _moduleRegistration2;
        private IModuleRegistration _moduleRegistration3;
        private ICommandServiceScopeFactory _commandServiceScopeFactory;
        private IServiceScope _serviceScope;

        [TestInitialize]
        public void Initialize()
        {
            _commandService = Substitute.For<ICommandService>();
            _serviceScope = Substitute.For<IServiceScope>();
            _commandServiceScopeFactory = Substitute.For<ICommandServiceScopeFactory>();
            _commandServiceScopeFactory.CreateScopeAsync(default!).ReturnsForAnyArgs(_serviceScope);
            _moduleRegistration1 = Substitute.For<IModuleRegistration>();
            _moduleRegistration2 = Substitute.For<IModuleRegistration>();
            _moduleRegistration3 = Substitute.For<IModuleRegistration>();
            _moduleRegistration1.ModuleType.Returns(typeof(TestModule1));
            _moduleRegistration2.ModuleType.Returns(typeof(TestModule2));
            _moduleRegistration3.ModuleType.Returns(typeof(TestModule3));
            _commandService.Modules.Returns(new ModuleInfo[] {default, default, default});
            var moduleRegistrations = new []{_moduleRegistration1, _moduleRegistration2, _moduleRegistration3};
            _sut = new CommandsModuleRegistrar(_commandServiceScopeFactory, _commandService, moduleRegistrations);
        }

        [TestMethod]
        public async Task StartAsync_Works()
        {
            await _sut.StartAsync(CancellationToken.None);
            await _commandService.Received().AddModuleAsync(_moduleRegistration1.ModuleType, _serviceScope.ServiceProvider);
            await _commandService.Received().AddModuleAsync(_moduleRegistration2.ModuleType, _serviceScope.ServiceProvider);
            await _commandService.Received().AddModuleAsync(_moduleRegistration3.ModuleType, _serviceScope.ServiceProvider);
            await _commandServiceScopeFactory.ReceivedWithAnyArgs().CreateScopeAsync(default!);
        }

        [TestMethod]
        public async Task StopAsync_Works()
        {
            await _sut.StartAsync(CancellationToken.None);
            await _sut.StopAsync(CancellationToken.None);
            await _commandService.ReceivedWithAnyArgs(3).RemoveModuleAsync(default(ModuleInfo));
        }

        private class TestModule3 : ModuleBase<ICommandContext>
        {
        }

        private class TestModule2 : ModuleBase<ICommandContext>
        {
        }

        private class TestModule1 : ModuleBase<ICommandContext>
        {
        }
    }
}