using Discord;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.Rules.Models
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