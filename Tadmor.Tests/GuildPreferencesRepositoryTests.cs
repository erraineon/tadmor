using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Services;
using Tadmor.Data.Interfaces;
using Tadmor.Data.Models;
using Tadmor.Data.Services;
using Tadmor.Notifications.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Preference.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class GuildPreferencesRepositoryTests
    {
        private const ulong ExistingGuildId = 12345UL;
        private const ulong NonExistingGuildId = 54321UL;
        private const ulong ExistingChannelId = 98765UL;
        private const ulong ExistingRoleId = 98765UL;
        private const ulong ExistingUserId = 98765UL;
        private GuildPreferencesRepository _sut;
        private INotificationPublisher _notificationPublisher;
        private ITadmorDbContext _dbContext;

        [TestInitialize]
        public void Initialize()
        {
            _dbContext = Substitute.For<ITadmorDbContext>();
            var existingPreferences = new GuildPreferencesEntity
            {
                GuildId = ExistingGuildId,
                Preferences = new GuildPreferences
                {
                    CommandPrefix = "!"
                }
            };
            _dbContext.GuildPreferences
                .FindAsync(ExistingGuildId)
                .Returns(existingPreferences);
            _dbContext.GuildPreferences.Add(existingPreferences);
            _notificationPublisher = Substitute.For<INotificationPublisher>();
            _sut = new GuildPreferencesRepository(_dbContext, _notificationPublisher);
        }

        [TestMethod]
        public async Task GetGuildPreferences_Works()
        {
            var guildPreferences = await _sut.GetGuildPreferencesAsyncOrNull(ExistingGuildId);
            Assert.IsNotNull(guildPreferences);
            Assert.AreEqual("!", guildPreferences.CommandPrefix);
            var nullGuildPreferences = await _sut.GetGuildPreferencesAsyncOrNull(NonExistingGuildId);
            Assert.IsNull(nullGuildPreferences);
        }

        [TestMethod]
        public async Task Update_Creates_When_Not_existing()
        {
            await _sut.UpdatePreferencesAsync(
                p => p.CommandPrefix = "!",
                NonExistingGuildId,
                new PreferencesScope(default, default, default));
            await _dbContext.GuildPreferences.ReceivedWithAnyArgs().AddAsync(default!);
        }

        [TestMethod]
        public async Task Update_Channel_Scope_Works()
        {
            await _sut.UpdatePreferencesAsync(
                p => p.CommandPrefix = "#",
                ExistingGuildId,
                new PreferencesScope(ExistingChannelId, default, default));
            var guildPreferences = await _dbContext.GuildPreferences.FindAsync(ExistingGuildId);
            Assert.IsNotNull(guildPreferences);
            Assert.IsTrue(guildPreferences.Preferences.ChannelPreferences.TryGetValue(ExistingChannelId, out var channelPreferences));
            Assert.AreEqual("#", channelPreferences.CommandPrefix);
        }

        [TestMethod]
        public async Task Update_User_Scope_Works()
        {
            await _sut.UpdatePreferencesAsync(
                p => p.CommandPrefix = "@",
                ExistingGuildId,
                new PreferencesScope(default, ExistingUserId, default));
            var guildPreferences = await _dbContext.GuildPreferences.FindAsync(ExistingGuildId);
            Assert.IsNotNull(guildPreferences);
            Assert.IsTrue(guildPreferences.Preferences.UserPreferences.TryGetValue(ExistingUserId, out var userPreferences));
            Assert.AreEqual("@", userPreferences.CommandPrefix);
        }

        [TestMethod]
        public async Task Update_Channel_Role_Scope_Works()
        {
            await _sut.UpdatePreferencesAsync(
                p => p.CommandPrefix = "$",
                ExistingGuildId,
                new PreferencesScope(ExistingChannelId, default, ExistingRoleId));
            var guildPreferences = await _dbContext.GuildPreferences.FindAsync(ExistingGuildId);
            Assert.IsNotNull(guildPreferences);
            Assert.IsTrue(guildPreferences.Preferences.ChannelPreferences
                .TryGetValue(ExistingChannelId, out var channelPreferences));
            Assert.IsTrue(channelPreferences.RolePreferences
                .TryGetValue(ExistingRoleId, out var rolePreferences));
            Assert.AreEqual("$", rolePreferences.CommandPrefix);
        }
    }
}