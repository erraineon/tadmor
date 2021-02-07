namespace Tadmor.Core.Preference.Models
{
    public sealed record PreferencesScope(ulong? ChannelId, ulong? UserId, ulong? RoleId);
}