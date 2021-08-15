using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Preference.Interfaces
{
    public interface IContextualPreferencesProvider
    {
        Task<Preferences> GetContextualPreferencesAsync(IGuildChannel channel, IGuildUser user);
    }
}