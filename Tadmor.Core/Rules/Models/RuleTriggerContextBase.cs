using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.Rules.Interfaces;

namespace Tadmor.Core.Rules.Models
{
    public abstract class RuleTriggerContextBase : IRuleTriggerContext
    {
        protected RuleTriggerContextBase(
            IUser executeAs,
            IGuildChannel executeIn,
            IChatClient chatClient,
            IUserMessage? referencedMessage,
            RuleBase rule, 
            bool shouldEvaluateSubCommands)
        {
            ExecuteAs = executeAs;
            ExecuteIn = executeIn;
            ChatClient = chatClient;
            ReferencedMessage = referencedMessage;
            Rule = rule;
            ShouldEvaluateSubCommands = shouldEvaluateSubCommands;
        }

        public IUser ExecuteAs { get; }
        public IGuildChannel ExecuteIn { get; }
        public IChatClient ChatClient { get; }
        public IUserMessage? ReferencedMessage { get; }
        public RuleBase Rule { get; }
        public bool ShouldEvaluateSubCommands { get; }
        public abstract bool ShouldExecute { get; }
    }
}