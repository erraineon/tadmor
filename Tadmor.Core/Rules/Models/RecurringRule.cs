using System;

namespace Tadmor.Core.Rules.Models
{
    public record RecurringRule(TimeSpan Interval, ulong AuthorUserId, string Reaction) : TimeRule(AuthorUserId, Reaction);
}