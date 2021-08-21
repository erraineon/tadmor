using System;

namespace Tadmor.Core.Rules.Models
{
    public record OneTimeRule(TimeSpan Delay, string Reaction) : TimeRule(Reaction);
}