using System.ComponentModel;
using System.Text;
using Discord;
using Discord.Commands;
using JetBrains.Annotations;

namespace Tadmor.Core.Preference.Models
{
    [NamedArgumentType]
    public class PreferencesScopeCommandModel
    {
        [DefaultValue("#chan")]
        public ITextChannel? Channel { get; [UsedImplicitly] init; }
        [DefaultValue("@user")]
        public IUser? User { get; [UsedImplicitly] init; }
        [DefaultValue("@role")]
        public IRole? Role { get; [UsedImplicitly] init; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (User != null) sb.Append(User.Username);
            else if (Role != null) sb.Append($"role {Role.Name}");
            else sb.Append("all users");
            if (Channel != null) sb.Append($" in {Channel.Mention}");
            return sb.ToString();
        }
    }
}