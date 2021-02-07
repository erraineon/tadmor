using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Formatting.Interfaces;
using Tadmor.Core.Preference.Models;

namespace Tadmor.Core.Commands.Formatters
{
    public class PreferenceScopeFormatter : IStringFormatter<PreferencesScope>
    {
        private readonly ICommandContext _context;

        public PreferenceScopeFormatter(ICommandContext context)
        {
            _context = context;
        }

        public async Task<string> ToStringAsync(PreferencesScope value)
        {
            var channelName = value.ChannelId is { } channelId
                ? (await _context.Guild.GetTextChannelAsync(channelId))?.Mention ?? "missing-channel"
                : default;
            var roleName = value.RoleId is { } roleId
                ? _context.Guild.GetRole(roleId).Name ?? "missing-role"
                : default;
            var userName = value.UserId is { } userId
                ? (await _context.Guild.GetUserAsync(userId)).Username ?? "missing-user"
                : default;

            string result;
            if (channelName == default && roleName == default && userName == default)
            {
                result = string.Empty;
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append(" (");
                if (channelName != default) sb.Append($"channel: {channelName}");
                if (roleName != default) sb.Append($"role: {roleName}");
                if (userName != default) sb.Append($"username: {userName}");
                sb.Append(')');
                result = sb.ToString();
            }

            return result;
        }
    }
}