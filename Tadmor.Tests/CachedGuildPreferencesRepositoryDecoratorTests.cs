using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;
using Tadmor.Core.Preference.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class CachedGuildPreferencesRepositoryDecoratorTests
    {
        private const ulong ExistentGuildId = 12345UL;
        private const ulong NonExistentGuildId = 98765UL;
        private GuildPreferences _guildPreferences;
        private IGuildPreferencesRepository _guildPreferencesRepository;
        private IMemoryCache _memoryCache;
        private CachedGuildPreferencesRepositoryDecorator _sut;

        [TestInitialize]
        public void Initialize()
        {
            _memoryCache = TestUtilities.CreateMemoryCache();
            _guildPreferencesRepository = Substitute.For<IGuildPreferencesRepository>();
            _guildPreferences = new GuildPreferences
            {
                CommandPrefix = "!"
            };
            _guildPreferencesRepository
                .GetGuildPreferencesAsyncOrNull(ExistentGuildId)
                .Returns(_guildPreferences);
            _sut = new CachedGuildPreferencesRepositoryDecorator(_guildPreferencesRepository, _memoryCache);
        }

        [TestMethod]
        public async Task GetGuildPreferencesAsync_Cache_Works()
        {
            await _sut.GetGuildPreferencesAsyncOrNull(ExistentGuildId);
            await _sut.GetGuildPreferencesAsyncOrNull(ExistentGuildId);
            await _guildPreferencesRepository.Received(1).GetGuildPreferencesAsyncOrNull(ExistentGuildId);
        }

        [TestMethod]
        public async Task UpdatePreferencesAsync_Cache_Clear_Works()
        {
            await _sut.GetGuildPreferencesAsyncOrNull(ExistentGuildId);
            await _sut.UpdatePreferencesAsync(_ => { }, ExistentGuildId,
                new PreferencesScope(default, default, default));
            await _sut.GetGuildPreferencesAsyncOrNull(ExistentGuildId);
            await _guildPreferencesRepository.Received(2).GetGuildPreferencesAsyncOrNull(ExistentGuildId);
            await _sut.UpdatePreferencesAsync(_ => { }, NonExistentGuildId,
                new PreferencesScope(default, default, default));
            await _sut.GetGuildPreferencesAsyncOrNull(ExistentGuildId);
            await _guildPreferencesRepository.Received(2).GetGuildPreferencesAsyncOrNull(ExistentGuildId);
        }
    }
}