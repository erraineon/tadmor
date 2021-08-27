using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Interfaces
{
    public interface IRuleTriggerContext
    {
        IUser ExecuteAs { get; }
        IGuildChannel ExecuteIn { get; }
        IChatClient ChatClient { get; }
        IUserMessage? ReferencedMessage { get; }
        RuleBase Rule { get; }
        bool ShouldEvaluateSubCommands { get; }
    }
}