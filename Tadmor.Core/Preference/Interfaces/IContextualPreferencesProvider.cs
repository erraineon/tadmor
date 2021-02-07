using System.Threading.Tasks;
using Discord;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Preference.Interfaces
{
    public interface IContextualPreferencesProvider
    {
        Task<Preferences> GetContextualPreferences(IGuildChannel channel, IGuildUser user);
    }
}