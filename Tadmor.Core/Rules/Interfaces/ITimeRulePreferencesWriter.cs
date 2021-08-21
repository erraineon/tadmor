using System;
using System.Threading.Tasks;
using Tadmor.Core.Preference.Models;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Interfaces
{
    public interface ITimeRulePreferencesWriter
    {
        Task<TimeRule> UpdatePreferencesAsync(
            ulong guildId,
            ulong channelId,
            TimeRule timeRule,
            Action<Preferences, TimeRule> updateAction);
    }
}