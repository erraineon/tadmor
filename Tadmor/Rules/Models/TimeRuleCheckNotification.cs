using Tadmor.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Rules.Models
{
    public record TimeRuleCheckNotification(IChatClient ChatClient);
}