using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Abstractions.Interfaces;
using Tadmor.Abstractions.Services;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Models;

namespace Tadmor.Tests
{
    [TestClass]
    public class ChatClientEventDispatcherTests
    {
        private IChatClient _client1;
        private IChatClient _client2;
        private INotificationPublisherFactory _notificationPublisherFactory;
        private ChatClientEventDispatcher _sut;
        private INotificationPublisher _notificationPublisher;

        [TestInitialize]
        public void Initialize()
        {
            _client1 = Substitute.For<IChatClient>();
            _client2 = Substitute.For<IChatClient>();
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
        public async Task StopAsync_GuildMemberUpdated_Works()
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
    }
}