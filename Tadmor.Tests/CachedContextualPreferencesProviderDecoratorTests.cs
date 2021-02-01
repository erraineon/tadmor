using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Abstractions.Models;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Preference.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class CachedContextualPreferencesProviderDecoratorTests
    {
        private const ulong ExistingUserId = 12345UL;
        private IContextualPreferencesProvider _contextualPreferencesProvider;
        private IMemoryCache _memoryCache;
        private CachedContextualPreferencesProviderDecorator _sut;

        [TestInitialize]
        public void Initialize()
        {
            _contextualPreferencesProvider = Substitute.For<IContextualPreferencesProvider>();
            _memoryCache = TestUtilities.CreateMemoryCache();
            _sut = new CachedContextualPreferencesProviderDecorator(_contextualPreferencesProvider, _memoryCache);
        }

        [TestMethod]
        public async Task Cache_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            var guildUser = Substitute.For<IGuildUser>();
            guildUser.Id.Returns(ExistingUserId);
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            await _contextualPreferencesProvider.Received(1).GetContextualPreferences(guildChannel, guildUser);
        }

        [TestMethod]
        public async Task GuildMember_Cache_Eviction_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            var guildUser = Substitute.For<IGuildUser>();
            guildUser.Id.Returns(ExistingUserId);
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            var chatClient = Substitute.For<IChatClient>();
            var newUser = Substitute.For<IGuildUser>();
            newUser.Id.Returns(ExistingUserId);
            newUser.RoleIds.Returns(new[] {98765UL});
            await _sut.HandleAsync(new GuildMemberUpdatedNotification(chatClient, guildUser, newUser),
                CancellationToken.None);
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            await _contextualPreferencesProvider.Received(2).GetContextualPreferences(guildChannel, guildUser);
        }

        [TestMethod]
        public async Task GuildMember_Cache_No_Eviction_When_Roles_Unchanged()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            var guildUser = Substitute.For<IGuildUser>();
            guildUser.Id.Returns(ExistingUserId);
            guildUser.RoleIds.Returns(new[] {98765UL});
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            var chatClient = Substitute.For<IChatClient>();
            var newUser = Substitute.For<IGuildUser>();
            newUser.Id.Returns(ExistingUserId);
            newUser.RoleIds.Returns(new[] {98765UL});
            var notification = new GuildMemberUpdatedNotification(chatClient, guildUser, newUser);
            await _sut.HandleAsync(notification, CancellationToken.None);
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            await _contextualPreferencesProvider.Received(1).GetContextualPreferences(guildChannel, guildUser);
        }

        [TestMethod]
        public async Task Guild_Eviction_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            var guildUser = Substitute.For<IGuildUser>();
            guildUser.Id.Returns(ExistingUserId);
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            var notification = new GuildPreferencesUpdatedNotification(guildUser.GuildId, new Preferences());
            await _sut.HandleAsync(notification, CancellationToken.None);
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            await _contextualPreferencesProvider.Received(2).GetContextualPreferences(guildChannel, guildUser);
        }
    }
}