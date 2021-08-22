using System;

namespace Tadmor.Core.Rules.Models
{
    public abstract record TimeRule(ulong AuthorUserId, string Reaction) : RuleBase(Reaction)
    {
        public DateTime? LastRunDate { get; init; }
        public DateTime NextRunDate { get; init; }
    }
}