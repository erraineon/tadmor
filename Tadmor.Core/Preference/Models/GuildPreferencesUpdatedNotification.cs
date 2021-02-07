namespace Tadmor.Core.Preference.Models
{
    public sealed record GuildPreferencesUpdatedNotification(ulong GuildId, Preferences Preferences);
}