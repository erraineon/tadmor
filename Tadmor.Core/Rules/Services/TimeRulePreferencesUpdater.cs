using System;
using System.Threading.Tasks;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Services
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