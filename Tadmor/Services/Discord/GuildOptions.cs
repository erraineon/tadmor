using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Discord.WebSocket;

namespace Tadmor.Services.Discord
{
    public class GuildOptions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        public string CommandPrefix { get; set; }
        public string WelcomeMessage { get; set; }
        public ulong WelcomeChannel { get; set; }
        public List<GuildEvent> Events { get; set; } = new List<GuildEvent>();
    }

    public class GuildEvent
    {
        public GuildEventScope Scope { get; set; }
        public GuildEventTriggerType TriggerType { get; set; }
        public string Trigger { get; set; }
        public bool DeleteTrigger { get; set; }
        public string Reaction { get; set; }
        public ulong ChannelId { get; set; }
        public string Id { get; set; }

        public string ToString(SocketGuild guild)
        {
            switch (TriggerType)
            {
                case GuildEventTriggerType.GuildJoin:
                    var channelMention = (guild.GetChannel(ChannelId) as SocketTextChannel)?.Mention ?? "missing channel";
                    return $"{Id}: when users join this guild, execute '{Reaction}' in {channelMention}";
                case GuildEventTriggerType.RegexMatch:
                    var function = DeleteTrigger ? "delete and execute" : "execute";
                    return $"{Id}: when users say '{Trigger}' {function} '{Reaction}'";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum GuildEventScope
    {
        None,
        Channel,
        Guild
    }

    public enum GuildEventTriggerType
    {
        None, 
        GuildJoin,
        RegexMatch
    }
}