using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;

namespace Tadmor.Modules
{
    [RequireBotPermission(ChannelPermission.CreateInstantInvite)]
    [RequireOwner]
    public class InviteUrlModule : ModuleBase<ICommandContext>
    {
        [Command("inviteurl")]
        public async Task CreateInviteUrl(params string[] words)
        {
            var options = new RequestOptions {RetryMode = RetryMode.RetryRatelimit};
            await ReplyAsync($"searching for {words.Humanize()}");
            var channel = (SocketGuildChannel) Context.Channel;
            RestInviteMetadata invite;
            while (true)
            {
                invite = await channel.CreateInviteAsync(null, isUnique: true, options: options);
                if (words.Any(w => invite.Url.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0)) break;
                Console.WriteLine(invite.Url);
                await invite.DeleteAsync(options);
            }

            await ReplyAsync(invite.Url);
        }
    }
}