using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.ChatClients.Abstractions.Models;
using Tadmor.Core.ChatClients.Abstractions.Services;
using Tadmor.Core.Notifications.Interfaces;

namespace Tadmor.Tests
{
    [TestClass]
    public class ChatClientEventDispatcherTests
    {
        private IChatEventProvider _client1;
        private IChatEventProvider _client2;
        private INotificationPublisherFactory _notificationPublisherFactory;
        private ChatClientEventDispatcher _sut;
        private INotificationPublisher _notificationPublisher;

        [TestInitialize]
        public void Initialize()
        {
            _client1 = Substitute.For<IChatEventProvider>();
            _client2 = Substitute.For<IChatEventProvider>();
            _notificationPublisherFactory = Substitute.For<INotificationPublisherFactory>();
            _notificationPublisher = Substitute.For<INotificationPublisher>();
            _notificationPublisherFactory.Create().Returns(_notificationPublisher);
            _sut = new ChatClientEventDispatcher(new []{ _client1, _client2 }, _notificationPublisherFactory);
        }

        [TestMethod]
        public async Task StartAsync_MessageReceived_Works()
        {
            await _sut.StartAsync(CancellationToken.None);
            var message = Substitute.For<IMessage>();
            _client1.MessageReceived += Raise.Event<Func<IChatClient, IMessage, Task>>(_client1, message);
            _client2.MessageReceived += Raise.Event<Func<IChatClient, IMessage, Task>>(_client2, message);
            _client2.MessageReceived += Raise.Event<Func<IChatClient, IMessage, Task>>(_client2, message);
            await _notificationPublisher
                .Received(3)
                .PublishAsync(Arg.Any<MessageReceivedNotification>());
        }

        [TestMethod]
        public async Task StopAsync_MessageReceived_Works()
        {
            await _sut.StartAsync(CancellationToken.None);
            await _sut.StopAsync(CancellationToken.None);
            var message = Substitute.For<IMessage>();
            _client1.MessageReceived += Raise.Event<Func<IChatClient, IMessage, Task>>(_client1, message);
            _client2.MessageReceived += Raise.Event<Func<IChatClient, IMessage, Task>>(_client2, message);
            _client2.MessageReceived += Raise.Event<Func<IChatClient, IMessage, Task>>(_client2, message);
            await _notificationPublisher
                .Received(0)
                .PublishAsync(Arg.Any<MessageReceivedNotification>());
        }

        [TestMethod]
        public async Task StartAsync_GuildMemberUpdated_Works()
        {
            await _sut.StartAsync(CancellationToken.None);
            var oldUser = Substitute.For<IGuildUser>();
            var newUser = Substitute.For<IGuildUser>();
            _client1.GuildMemberUpdated += Raise.Event<Func<IChatClient, IGuildUser, IGuildUser, Task>>(_client1, oldUser, newUser);
            _client2.GuildMemberUpdated += Raise.Event<Func<IChatClient, IGuildUser, IGuildUser, Task>>(_client2, oldUser, newUser);
            _client2.GuildMemberUpdated += Raise.Event<Func<IChatClient, IGuildUser, IGuildUser, Task>>(_client2, oldUser, newUser);
            await _notificationPublisher
                .Received(3)
                .PublishAsync(Arg.Any<GuildMemberUpdatedNotification>());
        }

        [TestMethod]
        public async Task StopAsync_Log_Works()
        {
            await _sut.StartAsync(CancellationToken.None);
            await _sut.StopAsync(CancellationToken.None);
            var oldUser = Substitute.For<IGuildUser>();
            var newUser = Substitute.For<IGuildUser>();
            _client1.GuildMemberUpdated += Raise.Event<Func<IChatClient, IGuildUser, IGuildUser, Task>>(_client1, oldUser, newUser);
            _client2.GuildMemberUpdated += Raise.Event<Func<IChatClient, IGuildUser, IGuildUser, Task>>(_client2, oldUser, newUser);
            _client2.GuildMemberUpdated += Raise.Event<Func<IChatClient, IGuildUser, IGuildUser, Task>>(_client2, oldUser, newUser);
            await _notificationPublisher
                .Received(0)
                .PublishAsync(Arg.Any<GuildMemberUpdatedNotification>());
        }

        [TestMethod]
        public async Task StartAsync_Log_Works()
        {
            await _sut.StartAsync(CancellationToken.None);
            var logMessage = new LogMessage();
            _client1.Log += Raise.Event<Func<IChatClient, LogMessage, Task>>(_client1, logMessage);
            _client2.Log += Raise.Event<Func<IChatClient, LogMessage, Task>>(_client2, logMessage);
            _client2.Log += Raise.Event<Func<IChatClient, LogMessage, Task>>(_client2, logMessage);
            await _notificationPublisher
                .Received(3)
                .PublishAsync(Arg.Any<LogNotification>());
        }

        [TestMethod]
        public async Task StopAsync_GuildMemberUpdated_Works()
        {
            await _sut.StartAsync(CancellationToken.None);
            await _sut.StopAsync(CancellationToken.None);
            var logMessage = new LogMessage();
            _client1.Log += Raise.Event<Func<IChatClient, LogMessage, Task>>(_client1, logMessage);
            _client2.Log += Raise.Event<Func<IChatClient, LogMessage, Task>>(_client2, logMessage);
            _client2.Log += Raise.Event<Func<IChatClient, LogMessage, Task>>(_client2, logMessage);
            await _notificationPublisher
                .Received(0)
                .PublishAsync(Arg.Any<LogNotification>());
        }
    }
}