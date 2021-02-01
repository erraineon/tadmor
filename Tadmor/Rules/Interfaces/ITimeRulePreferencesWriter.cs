using System;
using System.Threading.Tasks;
using Tadmor.Preference.Models;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Interfaces
{
    public interface ITimeRulePreferencesWriter
    {
        Task<TimeRule> UpdatePreferencesAsync(ulong guildId, ulong channelId, TimeRule timeRule, Action<Preferences, TimeRule> updateAction);
    }
}