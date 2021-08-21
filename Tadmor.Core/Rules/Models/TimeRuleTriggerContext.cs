using System;
using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.Rules.Models
{
    public class TimeRuleTriggerContext : RuleTriggerContextBase
    {
        private readonly TimeRule _timeRule;

        public TimeRuleTriggerContext(
            TimeRule timeRule,
            IUser executeAs,
            IGuildChannel executeIn,
            IChatClient chatClient) : base(executeAs, executeIn, chatClient, default, timeRule)
        {
            _timeRule = timeRule;
        }

        public override bool ShouldExecute => _timeRule.NextRunDate <= DateTime.Now;
    }
}