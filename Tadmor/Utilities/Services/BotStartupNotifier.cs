using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.ChatClients.Discord.Interfaces;
using Tadmor.Core.ChatClients.Discord.Notifications;
using Tadmor.Core.Notifications.Interfaces;

namespace Tadmor.Utilities.Services
{
    public class BotStartupNotifier : INotificationHandler<ChatClientReadyNotification>
    {
        public async Task HandleAsync(ChatClientReadyNotification notification, CancellationToken cancellationToken)
        {
            if (notification.ChatClient is IDiscordChatClient discord)
            {
                var applicationInfo = await discord.GetApplicationInfoAsync();
                await applicationInfo.Owner.SendMessageAsync($"started at {DateTime.Now:g}");
            }
        }
    }
}