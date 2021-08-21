using System;

namespace Tadmor.Core.Rules.Models
{
    public record RecurringRule(TimeSpan Interval, string Reaction) : TimeRule(Reaction);
}