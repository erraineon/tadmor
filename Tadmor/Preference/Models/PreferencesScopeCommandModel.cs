using System.Text;
using Discord;
using Discord.Commands;
using JetBrains.Annotations;

namespace Tadmor.Preference.Models
{
    [NamedArgumentType]
    public class PreferencesScopeCommandModel
    {
        public ITextChannel? Channel { get; [UsedImplicitly] init; }
        public IUser? User { get; [UsedImplicitly] init; }
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