using System;

namespace Tadmor.Services.Discord
{
    public class GuildEvent
    {
        public GuildEventScope Scope { get; set; }
        public GuildEventTriggerType TriggerType { get; set; }
        public string? Trigger { get; set; }
        public bool DeleteTrigger { get; set; }
        public string? Reaction { get; set; }
        public ulong ChannelId { get; set; }
        public string? Id { get; set; }
        public ulong? SenderIdFilter { get; set; }
        public int? MessagesCount { get; set; }

        public override string ToString()
        {
            var user = SenderIdFilter.HasValue ? $"user {SenderIdFilter}" : "users";
            switch (TriggerType)
            {
                case GuildEventTriggerType.GuildJoin:
                    return $"{Id}: when {user} join this guild, execute '{Reaction}'";
                case GuildEventTriggerType.RegexMatch:
                    var function = DeleteTrigger ? "delete and execute" : "execute";
                    return $"{Id}: when {user} say `{Trigger}`, {function} '{Reaction}'";
                case GuildEventTriggerType.EverySoOften:
                    return $"{Id}: every {MessagesCount} messages, execute '{Reaction}'";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}