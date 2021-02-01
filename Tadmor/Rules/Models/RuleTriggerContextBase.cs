using Discord;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Rules.Interfaces;

namespace Tadmor.Rules.Models
{
    public abstract class RuleTriggerContextBase : IRuleTriggerContext
    {
        public IUser ExecuteAs { get; }
        public IGuildChannel ExecuteIn { get; }
        public IChatClient ChatClient { get; }
        public IUserMessage? ReferencedMessage { get; }
        public RuleBase Rule { get; }

        protected RuleTriggerContextBase(IUser executeAs,
            IGuildChannel executeIn,
            IChatClient chatClient,
            IUserMessage? referencedMessage, 
            RuleBase rule)
        {
            ExecuteAs = executeAs;
            ExecuteIn = executeIn;
            ChatClient = chatClient;
            ReferencedMessage = referencedMessage;
            Rule = rule;
        }
    }
}