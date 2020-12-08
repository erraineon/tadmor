using System.Threading.Tasks;
using Discord;
using Tadmor.Preference.Models;

namespace Tadmor.Preference.Interfaces
{
    public interface IContextualPreferencesProvider
    {
        Task<Preferences> GetContextualPreferences(IGuildChannel channel, IGuildUser user);
    }
}