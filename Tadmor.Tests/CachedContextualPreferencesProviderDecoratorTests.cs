using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Services;
using Tadmor.Notifications.Models;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class CachedContextualPreferencesProviderDecoratorTests
    {
        private const ulong ExistingUserId = 12345UL;
        private CachedContextualPreferencesProviderDecorator _sut;
        private IContextualPreferencesProvider _contextualPreferencesProvider;
        private IMemoryCache _memoryCache;
        private ICacheEntry _cacheEntry;

        [TestInitialize]
        public void Initialize()
        {
            _contextualPreferencesProvider = Substitute.For<IContextualPreferencesProvider>();
            _memoryCache = Substitute.For<IMemoryCache>();
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
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            var chatClient = Substitute.For<IChatClient>();
            var newUser = Substitute.For<IGuildUser>();
            newUser.Id.Returns(ExistingUserId);
            newUser.RoleIds.Returns(new[]{98765UL});
            await _sut.HandleAsync(new GuildMemberUpdatedNotification(chatClient, guildUser, newUser),
                CancellationToken.None);
            await _sut.GetContextualPreferences(guildChannel, guildUser);
            await _contextualPreferencesProvider.Received(2).GetContextualPreferences(guildChannel, guildUser);
        }
    }
}