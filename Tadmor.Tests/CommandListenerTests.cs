using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Services;
using Tadmor.Notifications.Models;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;

namespace Tadmor.Tests
{
    [TestClass]
    public class CommandListenerTests
    {
        private CommandListener _sut;
        private IContextualPreferencesProvider _contextualPreferencesProvider;
        private ICommandExecutor _commandExecutor;
        private IChatClient _chatClient;

        [TestInitialize]
        public void Initialize()
        {
            _contextualPreferencesProvider = Substitute.For<IContextualPreferencesProvider>();
            _commandExecutor = Substitute.For<ICommandExecutor>();
            _chatClient = Substitute.For<IChatClient>();
            _sut = new CommandListener(_contextualPreferencesProvider, _commandExecutor);
        }

        [TestMethod]
        public async Task Handle_Works()
        {
            var message = Substitute.For<IUserMessage>();
            var channel = Substitute.For<IMessageChannel, IGuildChannel>();
            var author = Substitute.For<IGuildUser>();
            var preferences = new Preferences {CommandPrefix = "!"};
            _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
            message.Channel.Returns(channel);
            message.Author.Returns(author);
            message.Content.Returns("!test");
            await _sut.HandleAsync(new MessageReceivedNotification(_chatClient, message), CancellationToken.None);
            await _commandExecutor.ReceivedWithAnyArgs(1).BeginExecutionAsync(default!, default!);
        }

        [TestMethod]
        public async Task Handle_Rejects_Non_IUserMessage()
        {
            var message = Substitute.For<IMessage>();
            var channel = Substitute.For<IMessageChannel, IGuildChannel>();
            var author = Substitute.For<IGuildUser>();
            var preferences = new Preferences {CommandPrefix = "!"};
            _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
            message.Channel.Returns(channel);
            message.Author.Returns(author);
            message.Content.Returns("!test");
            await _sut.HandleAsync(new MessageReceivedNotification(_chatClient, message), CancellationToken.None);
            await _commandExecutor.ReceivedWithAnyArgs(0).BeginExecutionAsync(default!, default!);
        }

        [TestMethod]
        public async Task Handle_Rejects_Non_IGuildChannel()
        {
            var message = Substitute.For<IUserMessage>();
            var channel = Substitute.For<IMessageChannel, IDMChannel>();
            var author = Substitute.For<IGuildUser>();
            var preferences = new Preferences {CommandPrefix = "!"};
            _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
            message.Channel.Returns(channel);
            message.Author.Returns(author);
            message.Content.Returns("!test");
            await _sut.HandleAsync(new MessageReceivedNotification(_chatClient, message), CancellationToken.None);
            await _commandExecutor.ReceivedWithAnyArgs(0).BeginExecutionAsync(default!, default!);
        }

        [TestMethod]
        public async Task Handle_Rejects_Non_IGuildUser()
        {
            var message = Substitute.For<IUserMessage>();
            var channel = Substitute.For<IMessageChannel, IDMChannel>();
            var author = Substitute.For<IGroupUser>();
            var preferences = new Preferences {CommandPrefix = "!"};
            _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
            message.Channel.Returns(channel);
            message.Author.Returns(author);
            message.Content.Returns("!test");
            await _sut.HandleAsync(new MessageReceivedNotification(_chatClient, message), CancellationToken.None);
            await _commandExecutor.ReceivedWithAnyArgs(0).BeginExecutionAsync(default!, default!);
        }

        [TestMethod]
        public async Task Handle_Rejects_Wrong_Prefix()
        {
            var message = Substitute.For<IUserMessage>();
            var channel = Substitute.For<IMessageChannel, IDMChannel>();
            var author = Substitute.For<IGroupUser>();
            var preferences = new Preferences {CommandPrefix = "$"};
            _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
            message.Channel.Returns(channel);
            message.Author.Returns(author);
            message.Content.Returns("!test");
            await _sut.HandleAsync(new MessageReceivedNotification(_chatClient, message), CancellationToken.None);
            await _commandExecutor.ReceivedWithAnyArgs(0).BeginExecutionAsync(default!, default!);
        }
    }
}