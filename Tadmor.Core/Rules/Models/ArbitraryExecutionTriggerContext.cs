using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Rules.Models
{
    public class ArbitraryExecutionTriggerContext : RuleTriggerContextBase
    {
        public ArbitraryExecutionTriggerContext(
            string command,
            MessageValidatedNotification notification) : base(
            notification.GuildUser,
            notification.GuildChannel,
            notification.ChatClient,
            notification.UserMessage,
            new ArbitraryExecutionRule(command))
        {
        }

        public override bool ShouldExecute => true;

        private record ArbitraryExecutionRule(string Command) : RuleBase(Command);
    }
}