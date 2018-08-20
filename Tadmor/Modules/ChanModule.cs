using Discord;
using Discord.Commands;
using Humanizer;
using System.Linq;
using System.Threading.Tasks;
using Tadmor.Extensions;
using Tadmor.Preconditions;
using Tadmor.Services.Chan;

namespace Tadmor.Modules
{
    public class ChanModule : ModuleBase<ICommandContext>
    {
        private readonly ChanService _chan;

        public ChanModule(ChanService chan)
        {
            _chan = chan;
        }

        [Command("hot")]
        [Ratelimit(1, 1, Measure.Minutes, RatelimitFlags.ApplyPerGuild)]
        public async Task HotPosts(string boardName)
        {
            var hotPosts = await _chan.GetHotPosts(boardName, 10);
            var embed = new EmbedBuilder();
            foreach (var post in hotPosts.OrderByDescending(post => post.Replies))
            {
                var title = $"{post.ThreadUrlSlug} - {post.Replies} replies";
                if (post.Name != null) title = $"{title} - {post.Name.StripHtml()}";
                var description = (post.Comment ?? $"{post.OriginalFileName}{post.FileExtension}")
                    .StripHtml()
                    .Truncate(EmbedFieldBuilder.MaxFieldValueLength);
                embed.AddField(builder => builder.WithName(title).WithValue(description));
            }
            await ReplyAsync(string.Empty, embed: embed.Build());
        }
    }
}
