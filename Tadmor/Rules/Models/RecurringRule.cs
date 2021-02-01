using System;

namespace Tadmor.Rules.Models
{
    public record RecurringRule(TimeSpan Interval, string Reaction) : TimeRule(Reaction);
}