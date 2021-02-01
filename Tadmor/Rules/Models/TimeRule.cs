using System;

namespace Tadmor.Rules.Models
{
    public abstract record TimeRule : RuleBase
    {
        public DateTime? LastRunDate { get; init; }
        public DateTime NextRunDate { get; init; }

        protected TimeRule(string reaction) : base(reaction)
        {
        }
    }
}