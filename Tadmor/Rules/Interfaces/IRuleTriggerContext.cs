using Discord;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Interfaces
{
    public interface IRuleTriggerContext
    {
        IUser ExecuteAs { get; }
        IGuildChannel ExecuteIn { get; }
        IChatClient ChatClient { get; }
        IUserMessage? ReferencedMessage { get; }
        RuleBase Rule { get; }
    }
}