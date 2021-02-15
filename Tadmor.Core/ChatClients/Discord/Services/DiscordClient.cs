using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.ChatClients.Discord.Interfaces;

namespace Tadmor.Core.ChatClients.Discord.Services
{
    [ExcludeFromCodeCoverage]
    public class DiscordClient : DiscordSocketClient, IDiscordChatClient, IChatEventProvider
    {
        public DiscordClient() : base(new DiscordSocketConfig {MessageCacheSize = 100})
        {
            base.MessageReceived += message => MessageReceived(this, message);
            base.GuildMemberUpdated += (oldUser, newUser) => GuildMemberUpdated(this, oldUser, newUser);
            base.Log += logMessage => Log(this, logMessage);
        }

        public new event Func<IChatClient, IMessage, Task> MessageReceived =
            (_, _) => Task.CompletedTask;

        public new event Func<IChatClient, IGuildUser, IGuildUser, Task> GuildMemberUpdated =
            (_, _, _) => Task.CompletedTask;

        public new event Func<IChatClient, LogMessage, Task> Log = (_, _) => Task.CompletedTask;
        public string Name => "discord";
    }
}