using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.ChatClients.Interfaces;

namespace Tadmor.Commands.Notifications
{
    public record CommandExecutionRequestedNotification(IChatClient ChatClient, ICommandContext CommandContext,
        string Input);
}
