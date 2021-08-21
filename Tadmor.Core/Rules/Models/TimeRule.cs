using System;

namespace Tadmor.Core.Rules.Models
{
    public abstract record TimeRule : RuleBase
    {
        protected TimeRule(string reaction) : base(reaction)
        {
        }

        public DateTime? LastRunDate { get; init; }
        public DateTime NextRunDate { get; init; }
    }
}