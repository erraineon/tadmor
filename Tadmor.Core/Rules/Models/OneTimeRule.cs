using System;

namespace Tadmor.Core.Rules.Models
{
    public record OneTimeRule(TimeSpan Delay, ulong AuthorUserId, string Reaction) : TimeRule(AuthorUserId, Reaction);
}