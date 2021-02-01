using System;
using System.Linq;
using System.Threading.Tasks;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Services
{
    public class TimeRuleProvider : ITimeRuleProvider
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;

        public TimeRuleProvider(IGuildPreferencesRepository guildPreferencesRepository)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
        }

        public async Task<ILookup<ulong, TimeRule>> GetRulesByChannelId(ulong guildId, DateTime dueDate)
        {
            var guildPreferences = await _guildPreferencesRepository.GetGuildPreferencesAsyncOrNull(guildId) ??
                new GuildPreferences();
            var result = guildPreferences.ChannelPreferences
                .SelectMany(
                    kvp => kvp.Value.Rules.OfType<TimeRule>(),
                    (kvp, rule) => (
                        channelId: kvp.Key,
                        timeRule: rule))
                .Where(t => t.timeRule.NextRunDate <= dueDate)
                .ToLookup(t => t.channelId, t => t.timeRule);
            return result;
        }
    }
}