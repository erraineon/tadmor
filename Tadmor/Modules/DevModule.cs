using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Tadmor.Preconditions;

namespace Tadmor.Modules
{
    [Summary("utilities")]
    public class DevModule : ModuleBase<ICommandContext>
    {
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        [RequireWhitelist]
        [Command("nick")]
        public async Task ChangeNickname(IGuildUser user, [Remainder] string nickname)
        {
            if (user.Id == Context.User.Id) throw new Exception("you can't change your own nickname");
            if (string.Equals(user.Username, nickname, StringComparison.OrdinalIgnoreCase))
                throw new Exception("you can't reset people's nicknames");
            await user.ModifyAsync(gup => gup.Nickname = nickname);
            await ReplyAsync($"{user.Username}'s nickname has been changed to {user.Nickname}");
        }

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
        [RequireServiceUser(Group = "admin")]
        [Command("say")]
        public Task Say([Remainder] string message)
        {
            return ReplyAsync(message);
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
    }
}