﻿using Discord;
using Discord.Commands;
using Humanizer;
using System.Linq;
using System.Threading.Tasks;
using Tadmor.Preconditions;
using Tadmor.Services.FourChan;
using Tadmor.Utils;

namespace Tadmor.Modules
{
    [Summary("4chan")]
    public class ChanModule : ModuleBase<ICommandContext>
    {
        private readonly ChanService _chan;

        public ChanModule(ChanService chan)
        {
            _chan = chan;
        }

        [Summary("gets topmost replied-to posts on a board")]
        [Command("hot")]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        public async Task HotPosts(string boardName)
        {
            var hotPosts = await _chan.GetHotPosts(boardName, 5);
            var embed = new EmbedBuilder();
            foreach (var post in hotPosts.OrderByDescending(post => post.Replies))
            {
                var title = $"{post.Url} - {post.Replies} replies";
                if (post.Name != null) title += $" - {StringUtils.StripHtml(post.Name)}";
                var description = StringUtils.StripHtml(post.Comment ?? $"{post.OriginalFileName}{post.FileExtension}")
                    .Truncate(EmbedFieldBuilder.MaxFieldValueLength);
                embed.AddField(builder => builder.WithName(title).WithValue(description));
            }
            await ReplyAsync(string.Empty, embed: embed.Build());
        }
    }
}
