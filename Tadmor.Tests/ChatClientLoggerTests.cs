using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.ChatClients.Abstractions.Models;
using Tadmor.Core.ChatClients.Abstractions.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class ChatClientLoggerTests
    {
        private ILogger<ChatClientLogger> _logger;
        private ChatClientLogger _sut;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Substitute.For<ILogger<ChatClientLogger>>();
            _sut = new ChatClientLogger(_logger);
        }

        [TestMethod]
        public async Task HandleAsync_Works()
        {
            var logMessage = new LogMessage();
            await _sut.HandleAsync(new LogNotification(Substitute.For<IChatClient>(), logMessage),
                CancellationToken.None);
            _logger.Received().Log(LogLevel.Information, logMessage.ToString());
        }
    }
}