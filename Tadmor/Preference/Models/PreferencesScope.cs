namespace Tadmor.Preference.Models
{
    public sealed record PreferencesScope(ulong? ChannelId, ulong? UserId, ulong? RoleId);
}