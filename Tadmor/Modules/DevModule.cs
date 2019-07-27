using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Tadmor.Preconditions;

namespace Tadmor.Modules
{
    [Summary("utilities")]
    public class DevModule : ModuleBase<ICommandContext>
    {
        [RequireOwner]
        [Command("ping")]
        public Task Ping()
        {
            return ReplyAsync("pong");
        }

        [RequireOwner]
        [Command("uptime")]
        public Task Uptime()
        {
            return ReplyAsync((DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize());
        }

        [RequireOwner]
        [Command("guilds")]
        public async Task Guilds()
        {
            var guilds = await Context.Client.GetGuildsAsync();
            await ReplyAsync(guilds.Humanize(g => $"{g.Name} ({g.Id})"));
        }

        [RequireOwner]
        [Command("leave")]
        public async Task LeaveGuild(ulong guildId)
        {
            var guild = await Context.Client.GetGuildAsync(guildId);
            if (guild != null) await guild.LeaveAsync();
        }

        [Summary("make the bot say something")]
        [RequireOwner(Group = "admin")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [RequireFakeUserMessage(Group = "admin")]
        [Command("say")]
        public Task Say([Remainder] string message)
        {
            return ReplyAsync(message);
        }

        [RequireBotPermission(ChannelPermission.CreateInstantInvite)]
        [Command("inviteurl")]
        [RequireOwner]
        public async Task CreateInviteUrl(params string[] words)
        {
            var options = new RequestOptions {RetryMode = RetryMode.RetryRatelimit};
            await ReplyAsync($"searching for {words.Humanize()}");
            var channel = (ITextChannel) Context.Channel;
            IInviteMetadata invite;
            while (true)
            {
                invite = await channel.CreateInviteAsync(null, isUnique: true, options: options);
                if (words.Any(w => invite.Url.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0)) break;
                await invite.DeleteAsync(options);
            }

            await ReplyAsync(invite.Url);
        }

        [Summary("delete the specified number of messages from everyone")]
        [RequireUserPermission(ChannelPermission.ManageMessages, Group = "admin")]
        [RequireOwner(Group = "admin")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Command("prune")]
        public async Task Prune(int count)
        {
            var channel = (ITextChannel) Context.Channel;
            var messages = await channel.GetMessagesAsync(count).FlattenAsync();
            await channel.DeleteMessagesAsync(messages);
        }
        
        [Summary("delete the specified number of messages from a user")]
        [RequireUserPermission(ChannelPermission.ManageMessages, Group = "admin")]
        [RequireOwner(Group = "admin")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Command("prune")]
        public async Task Prune(IGuildUser user, int count)
        {
            var channel = (ITextChannel)Context.Channel;
            // search backwards until enough messages have been collected
            IMessage lastMessage = Context.Message;
            List<IMessage> messages = new List<IMessage>(count);
            while (messages.Count < count)
            {
                var remainingMessages = count - messages.Count;
                var nextMessages = await channel.GetMessagesAsync(lastMessage.Id, Direction.Before).Flatten().ToList();
                // only messages more recent than two weeks ago can be deleted
                DateTimeOffset twoWeeksAgo = DateTimeOffset.Now - new TimeSpan(14, 0, 0, 0);
                var newMessages = nextMessages.Where(m => m.Timestamp > twoWeeksAgo).ToList();
                // if there are no more messages, stop looking now
                if (newMessages.Count == 0)
                {
                    break;
                }
                lastMessage = newMessages.Last();
                var newTargetMessages = newMessages.Where(m => m.Author.Id == user.Id).Take(remainingMessages).ToList();
                messages.AddRange(newTargetMessages);
                // if some messages were thrown away as too old, don't ask for more messages
                if (newMessages.Count < nextMessages.Count)
                {
                    break;
                }
            }
            await channel.DeleteMessagesAsync(messages);
        }

        [Summary("delete recent messages from a user")]
        [RequireUserPermission(ChannelPermission.ManageMessages, Group = "admin")]
        [RequireOwner(Group = "admin")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Command("prune")]
        public async Task Prune(IGuildUser user, TimeSpan timeSpan)
        {
            var channel = (ITextChannel)Context.Channel;
            DateTimeOffset deleteStart = Context.Message.Timestamp - timeSpan;
            // search backwards until enough messages have been collected
            IMessage lastMessage = Context.Message;
            List<IMessage> messages = new List<IMessage>();
            while (true)
            {
                var nextMessages = await channel.GetMessagesAsync(lastMessage.Id, Direction.Before).Flatten().ToList();
                // only messages more recent than the delete start
                var newMessages = nextMessages.Where(m => m.Timestamp > deleteStart).ToList();
                // if there are no more messages, stop looking now
                if (newMessages.Count == 0)
                {
                    break;
                }
                lastMessage = newMessages.Last();
                var newTargetMessages = newMessages.Where(m => m.Author.Id == user.Id).ToList();
                messages.AddRange(newTargetMessages);
                // if some messages were thrown away as too old, don't ask for more messages
                if (newMessages.Count < nextMessages.Count)
                {
                    break;
                }
            }
            await channel.DeleteMessagesAsync(messages);
        }
    }
}