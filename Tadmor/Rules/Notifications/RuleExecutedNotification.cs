﻿using Tadmor.ChatClients.Interfaces;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Notifications
{
    public record RuleExecutedNotification(IChatClient ChatClient, ulong GuildId, ulong ChannelId, RuleBase Rule);
}