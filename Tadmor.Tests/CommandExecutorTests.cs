using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class CommandExecutorTests
    {
        private ICommandContext _commandContext;
        private CommandExecutor _sut;
        private IServiceScopeFactory _serviceScopeFactory;
        private ICommandService _commandService;
        private IServiceScope _serviceScope;

        [TestInitialize]
        public void Initialize()
        {
            _commandContext = Substitute.For<ICommandContext>();
            _serviceScope = Substitute.For<IServiceScope>();
            _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            _serviceScopeFactory.CreateScope().Returns(_serviceScope);
            _commandService = Substitute.For<ICommandService>();
            _commandContext = Substitute.For<ICommandContext>();
            _sut = new CommandExecutor(_serviceScopeFactory, _commandService);
        }

        [TestMethod]
        public async Task ExecuteAsync_Works()
        {
            await _sut.BeginExecutionAsync(_commandContext, "test");
            await _commandService.Received(1).ExecuteAsync(_commandContext, "test", _serviceScope.ServiceProvider);
            _commandService.CommandExecuted +=
                Raise.Event<Func<Optional<CommandInfo>, ICommandContext, IResult, Task>>(new Optional<CommandInfo>(default), _commandContext, default);
            _serviceScope.Received().Dispose();
        }

        [TestMethod]
        public async Task ExecuteAsync_Does_Not_Dispose_Wrong_Scope()
        {
            await _sut.BeginExecutionAsync(_commandContext, "test");
            await _commandService.Received(1).ExecuteAsync(_commandContext, "test", _serviceScope.ServiceProvider);
            var otherContext = Substitute.For<ICommandContext>();
            _commandService.CommandExecuted +=
                Raise.Event<Func<Optional<CommandInfo>, ICommandContext, IResult, Task>>(new Optional<CommandInfo>(default), otherContext, default);
            _serviceScope.DidNotReceive().Dispose();
        }
    }

    public class TestModule3 : ModuleBase<ICommandContext>
    {
    }

    public class TestModule2 : ModuleBase<ICommandContext>
    {
    }

    public class TestModule1 : ModuleBase<ICommandContext>
    {
    }
}