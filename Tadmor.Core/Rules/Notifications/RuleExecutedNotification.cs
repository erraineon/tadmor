using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Notifications
{
    public record RuleExecutedNotification(IChatClient ChatClient, ulong GuildId, ulong ChannelId, RuleBase Rule);
}