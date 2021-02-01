using System;
using System.Threading.Tasks;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Services
{
    public class TimeRulePreferencesUpdater : ITimeRulePreferencesWriter
    {
        private readonly IGuildPreferencesRepository _guildPreferencesRepository;
        private readonly ITimeRuleNextRunDateCalculator _timeRuleRunDateCalculator;

        public TimeRulePreferencesUpdater(
            IGuildPreferencesRepository guildPreferencesRepository,
            ITimeRuleNextRunDateCalculator timeRuleRunDateCalculator)
        {
            _guildPreferencesRepository = guildPreferencesRepository;
            _timeRuleRunDateCalculator = timeRuleRunDateCalculator;
        }

        public async Task<TimeRule> UpdatePreferencesAsync(
            ulong guildId,
            ulong channelId,
            TimeRule timeRule,
            Action<Preferences, TimeRule> updateAction)
        {
            var preferencesScope = new PreferencesScope(channelId, default, default);
            var updatedTimeRule = timeRule with
            {
                LastRunDate = DateTime.Now,
                NextRunDate = _timeRuleRunDateCalculator.GetNextRunDate(timeRule)
            };
            await _guildPreferencesRepository.UpdatePreferencesAsync(
                preferences => updateAction(preferences, updatedTimeRule),
                guildId,
                preferencesScope);
            return updatedTimeRule;
        }
    }
}