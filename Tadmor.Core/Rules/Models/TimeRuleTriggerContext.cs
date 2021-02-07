using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.Rules.Models
{
    public class TimeRuleTriggerContext : RuleTriggerContextBase
    {
        public TimeRuleTriggerContext(
            TimeRule timeRule,
            IUser executeAs,
            IGuildChannel executeIn,
            IChatClient chatClient) : base(executeAs, executeIn, chatClient, default, timeRule)
        {
        }
    }
}