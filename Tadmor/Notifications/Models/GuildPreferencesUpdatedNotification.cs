using Tadmor.Preference.Models;

namespace Tadmor.Notifications.Models
{
    public sealed record GuildPreferencesUpdatedNotification(ulong GuildId, Preferences Preferences);
}