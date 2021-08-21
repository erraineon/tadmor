using Tadmor.Core.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Core.Rules.Models
{
    public record TimeRuleCheckNotification(IChatClient ChatClient);
}