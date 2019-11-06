using System;

namespace Tadmor.Services.Discord
{
    public class GuildEvent
    {
        public GuildEventScope Scope { get; set; }
        public GuildEventTriggerType TriggerType { get; set; }
        public string? Trigger { get; set; }
        public bool DeleteTrigger { get; set; }
        public string Reaction { get; set; }
        public ulong ChannelId { get; set; }
        public string Id { get; set; }

        public override string ToString()
        {
            switch (TriggerType)
            {
                case GuildEventTriggerType.GuildJoin:
                    return $"{Id}: when users join this guild, execute '{Reaction}'";
                case GuildEventTriggerType.RegexMatch:
                    var function = DeleteTrigger ? "delete and execute" : "execute";
                    return $"{Id}: when users say `{Trigger}`, {function} '{Reaction}'";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}