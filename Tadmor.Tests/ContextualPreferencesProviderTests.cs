using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Commands.Models;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Preference.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class ContextualPreferencesProviderTests
    {
        private const ulong ExistentGuildId = 12345UL;
        private const ulong ExistentChannelId = 12345UL;
        private const ulong ExistentUserId = 12345UL;
        private const ulong ExistentRoleId = 12345UL;
        private const ulong ExistentChannelWithNestedPrefsId = 98765UL;
        private GuildPreferences _guildPreferences;
        private IGuildPreferencesRepository _guildPreferencesRepository;
        private ContextualPreferencesProvider _sut;

        [TestInitialize]
        public void Initialize()
        {
            _guildPreferencesRepository = Substitute.For<IGuildPreferencesRepository>();
            _guildPreferences = new GuildPreferences
            {
                CommandPrefix = "!",
                CommandPermissions =
                {
                    new CommandPermission
                    {
                        CommandPermissionType = CommandPermissionType.Blacklist
                    }
                },
                ChannelPreferences =
                {
                    [ExistentChannelId] = new ChannelPreferences
                    {
                        CommandPrefix = "$",
                        CommandPermissions =
                        {
                            new CommandPermission
                            {
                                CommandPermissionType = CommandPermissionType.Whitelist
                            }
                        }
                    },
                    [ExistentChannelWithNestedPrefsId] = new ChannelPreferences
                    {
                        CommandPrefix = "^",
                        UserPreferences =
                        {
                            [ExistentUserId] = new UserPreferences
                            {
                                CommandPrefix = "@",
                                CommandPermissions =
                                {
                                    new CommandPermission
                                    {
                                        CommandPermissionType = CommandPermissionType.Whitelist
                                    }
                                }
                            }
                        },
                        RolePreferences =
                        {
                            [ExistentRoleId] = new RolePreferences
                            {
                                CommandPrefix = "*",
                                CommandPermissions =
                                {
                                    new CommandPermission
                                    {
                                        CommandPermissionType = CommandPermissionType.None
                                    }
                                }
                            }
                        },
                        CommandPermissions =
                        {
                            new CommandPermission
                            {
                                CommandPermissionType = CommandPermissionType.Blacklist
                            }
                        }
                    }
                },
                UserPreferences =
                {
                    [ExistentUserId] = new UserPreferences
                    {
                        CommandPrefix = "#",
                        CommandPermissions =
                        {
                            new CommandPermission
                            {
                                CommandPermissionType = CommandPermissionType.Whitelist
                            }
                        }
                    }
                },
                RolePreferences =
                {
                    [ExistentRoleId] = new RolePreferences()
                    {
                        CommandPrefix = "%",
                        CommandPermissions =
                        {
                            new CommandPermission
                            {
                                CommandPermissionType = CommandPermissionType.None
                            }
                        }
                    }
                }
            };
            _guildPreferencesRepository
                .GetGuildPreferencesAsync(ExistentGuildId)
                .Returns(_guildPreferences);
            _sut = new ContextualPreferencesProvider(_guildPreferencesRepository);
        }

        [TestMethod]
        public async Task No_Preferences_Yields_Default()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            var nonExistentGuildId = 123456UL;
            guildChannel.GuildId.Returns(nonExistentGuildId);
            var guildUser = Substitute.For<IGuildUser>();
            var preferences = await _sut.GetContextualPreferences(guildChannel, guildUser);
            Assert.AreEqual(".", preferences.CommandPrefix);
            Assert.IsTrue(!preferences.AutoCommands.Any());
            Assert.IsTrue(!preferences.CommandPermissions.Any());
        }

        [TestMethod]
        public async Task Flattening_None_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            guildChannel.GuildId.Returns(ExistentGuildId);
            var guildUser = Substitute.For<IGuildUser>();
            var preferences = await _sut.GetContextualPreferences(guildChannel, guildUser);
            Assert.AreEqual("!", preferences.CommandPrefix);
            var actualPermissionType = preferences.CommandPermissions.Single().CommandPermissionType;
            Assert.AreEqual(CommandPermissionType.Blacklist, actualPermissionType);
        }

        [TestMethod]
        public async Task Flattening_Channel_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            guildChannel.GuildId.Returns(ExistentGuildId);
            guildChannel.Id.Returns(ExistentChannelId);
            var guildUser = Substitute.For<IGuildUser>();
            var preferences = await _sut.GetContextualPreferences(guildChannel, guildUser);
            Assert.AreEqual("$", preferences.CommandPrefix);
            var actualPermissionType = preferences.CommandPermissions.Single().CommandPermissionType;
            Assert.AreEqual(CommandPermissionType.Whitelist, actualPermissionType);
        }

        [TestMethod]
        public async Task Flattening_Role_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            guildChannel.GuildId.Returns(ExistentGuildId);
            guildChannel.Id.Returns(ExistentChannelId);
            var guildUser = Substitute.For<IGuildUser>();
            guildUser.RoleIds.Returns(new[] {ExistentRoleId});
            var preferences = await _sut.GetContextualPreferences(guildChannel, guildUser);
            Assert.AreEqual("%", preferences.CommandPrefix);
            var actualPermissionType = preferences.CommandPermissions.Single().CommandPermissionType;
            Assert.AreEqual(CommandPermissionType.Whitelist, actualPermissionType);
        }

        [TestMethod]
        public async Task Flattening_User_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            guildChannel.GuildId.Returns(ExistentGuildId);
            guildChannel.Id.Returns(ExistentChannelId);
            var guildUser = Substitute.For<IGuildUser>();
            guildUser.RoleIds.Returns(new[] {ExistentRoleId});
            guildUser.Id.Returns(ExistentUserId);
            var preferences = await _sut.GetContextualPreferences(guildChannel, guildUser);
            Assert.AreEqual("#", preferences.CommandPrefix);
            var actualPermissionType = preferences.CommandPermissions.Single().CommandPermissionType;
            Assert.AreEqual(CommandPermissionType.Whitelist, actualPermissionType);
        }

        [TestMethod]
        public async Task Flattening_Channel_Role_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            guildChannel.GuildId.Returns(ExistentGuildId);
            guildChannel.Id.Returns(ExistentChannelWithNestedPrefsId);
            var guildUser = Substitute.For<IGuildUser>();
            guildUser.RoleIds.Returns(new[] {ExistentRoleId});
            var preferences = await _sut.GetContextualPreferences(guildChannel, guildUser);
            Assert.AreEqual("*", preferences.CommandPrefix);
            var actualPermissionType = preferences.CommandPermissions.Single().CommandPermissionType;
            Assert.AreEqual(CommandPermissionType.Blacklist, actualPermissionType);
        }

        [TestMethod]
        public async Task Flattening_Channel_User_Works()
        {
            var guildChannel = Substitute.For<IGuildChannel>();
            guildChannel.GuildId.Returns(ExistentGuildId);
            guildChannel.Id.Returns(ExistentChannelWithNestedPrefsId);
            var guildUser = Substitute.For<IGuildUser>();
            guildUser.RoleIds.Returns(new[] {ExistentRoleId});
            guildUser.Id.Returns(ExistentUserId);
            var preferences = await _sut.GetContextualPreferences(guildChannel, guildUser);
            Assert.AreEqual("@", preferences.CommandPrefix);
            var actualPermissionType = preferences.CommandPermissions.Single().CommandPermissionType;
            Assert.AreEqual(CommandPermissionType.Whitelist, actualPermissionType);
        }
    }
}