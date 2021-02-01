using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;
using Tadmor.Commands.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class CommandExecutorTests
    {
        private ICommandContext _commandContext;
        private CommandExecutor _sut;
        private ICommandServiceScopeFactory _commandServiceScopeFactory;
        private ICommandService _commandService;
        private IServiceScope _serviceScope;
        private ICommandResultPublisher _commandResultPublisher;

        [TestInitialize]
        public void Initialize()
        {
            _commandContext = Substitute.For<ICommandContext>();
            _serviceScope = Substitute.For<IServiceScope>();
            _commandServiceScopeFactory = Substitute.For<ICommandServiceScopeFactory>();
            _commandResultPublisher = Substitute.For<ICommandResultPublisher>();
            _commandServiceScopeFactory.CreateScopeAsync(default!).ReturnsForAnyArgs(_serviceScope);
            _commandService = Substitute.For<ICommandService>();
            _sut = new CommandExecutor(_commandServiceScopeFactory, _commandService, _commandResultPublisher);
        }

        [TestMethod]
        public async Task ExecuteAsync_Works()
        {
            await _sut.ExecuteAsync(new ExecuteCommandRequest(_commandContext, "test"), CancellationToken.None);
            await _commandServiceScopeFactory.Received().CreateScopeAsync(_commandContext);
            await _commandService.Received().ExecuteAsync(_commandContext, "test", _serviceScope.ServiceProvider);
            _serviceScope.Received().Dispose();
        }
    }
}