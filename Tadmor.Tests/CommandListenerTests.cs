using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Services;
using Tadmor.Core.Preference.Interfaces;

namespace Tadmor.Tests
{
    [TestClass]
    public class CommandListenerTests
    {
        private ChatCommandMonitor _sut;
        private IContextualPreferencesProvider _contextualPreferencesProvider;
        private ICommandPermissionValidator _commandPermissionValidator;
        private IChatClient _chatClient;
        private ICommandPrefixValidator _commandPrefixValidator;

        //[TestInitialize]
        //public void Initialize()
        //{
        //    _commandPrefixValidator = Substitute.For<ICommandPrefixValidator>();
        //    _contextualPreferencesProvider = Substitute.For<IContextualPreferencesProvider>();
        //    _commandPermissionValidator = Substitute.For<ICommandPermissionValidator>();
        //    _chatClient = Substitute.For<IChatClient>();
        //    _sut = new ChatCommandMonitor(_commandPrefixValidator, _frontFacingCommandExecutor, _commandPermissionValidator);
        //}

        //[TestMethod]
        //public async Task Handle_Works()
        //{
        //    var message = Substitute.For<IUserMessage>();
        //    var channel = Substitute.For<IMessageChannel, IGuildChannel>();
        //    var author = Substitute.For<IGuildUser>();
        //    var preferences = new Preferences {CommandPrefix = "!"};
        //    _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
        //    message.Channel.Returns(channel);
        //    message.Author.Returns(author);
        //    message.Content.Returns("!test");
        //    var notification = new MessageValidatedNotification(_chatClient, message, (IGuildChannel) channel, author);
        //    await _sut.HandleAsync(notification, CancellationToken.None);
        //    await _frontFacingCommandExecutor.ReceivedWithAnyArgs(1).ExecuteAndPublishAsync(default!, default!);
        //}

        //[TestMethod]
        //public async Task Handle_Rejects_Non_IUserMessage()
        //{
        //    var message = Substitute.For<IMessage>();
        //    var channel = Substitute.For<IMessageChannel, IGuildChannel>();
        //    var author = Substitute.For<IGuildUser>();
        //    var preferences = new Preferences {CommandPrefix = "!"};
        //    _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
        //    message.Channel.Returns(channel);
        //    message.Author.Returns(author);
        //    message.Content.Returns("!test");
        //    await _sut.HandleAsync(new MessageReceivedNotification(_chatClient, message), CancellationToken.None);
        //    await _frontFacingCommandExecutor.ReceivedWithAnyArgs(0).ExecuteAsync(default!, default!);
        //}

        //[TestMethod]
        //public async Task Handle_Rejects_Non_IGuildChannel()
        //{
        //    var message = Substitute.For<IUserMessage>();
        //    var channel = Substitute.For<IMessageChannel, IDMChannel>();
        //    var author = Substitute.For<IGuildUser>();
        //    var preferences = new Preferences {CommandPrefix = "!"};
        //    _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
        //    message.Channel.Returns(channel);
        //    message.Author.Returns(author);
        //    message.Content.Returns("!test");
        //    await _sut.HandleAsync(new MessageReceivedNotification(_chatClient, message), CancellationToken.None);
        //    await _frontFacingCommandExecutor.ReceivedWithAnyArgs(0).ExecuteAsync(default!, default!);
        //}

        //[TestMethod]
        //public async Task Handle_Rejects_Non_IGuildUser()
        //{
        //    var message = Substitute.For<IUserMessage>();
        //    var channel = Substitute.For<IMessageChannel, IDMChannel>();
        //    var author = Substitute.For<IGroupUser>();
        //    var preferences = new Preferences {CommandPrefix = "!"};
        //    _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
        //    message.Channel.Returns(channel);
        //    message.Author.Returns(author);
        //    message.Content.Returns("!test");
        //    await _sut.HandleAsync(new MessageReceivedNotification(_chatClient, message), CancellationToken.None);
        //    await _frontFacingCommandExecutor.ReceivedWithAnyArgs(0).ExecuteAsync(default!, default!);
        //}

        //[TestMethod]
        //public async Task Handle_Rejects_Wrong_Prefix()
        //{
        //    var message = Substitute.For<IUserMessage>();
        //    var channel = Substitute.For<IMessageChannel, IGuildChannel>();
        //    var author = Substitute.For<IGuildUser>();
        //    var preferences = new Preferences {CommandPrefix = "$"};
        //    _contextualPreferencesProvider.GetContextualPreferences(default!, default!).ReturnsForAnyArgs(preferences);
        //    message.Channel.Returns(channel);
        //    message.Author.Returns(author);
        //    message.Content.Returns("!test");
        //    var notification = new MessageValidatedNotification(_chatClient, message, (IGuildChannel)channel, author);
        //    await _sut.HandleAsync(notification, CancellationToken.None);
        //    await _frontFacingCommandExecutor.ReceivedWithAnyArgs(0).ExecuteAndPublishAsync(default!, default!);
        //}
    }
}