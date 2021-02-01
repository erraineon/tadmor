using System;

namespace Tadmor.Rules.Models
{
    public record OneTimeRule(TimeSpan Delay, string Reaction) : TimeRule(Reaction);
}