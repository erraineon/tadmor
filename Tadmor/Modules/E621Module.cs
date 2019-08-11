using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Preconditions;
using Tadmor.Services.E621;

namespace Tadmor.Modules
{
    [Summary("e621")]
    public class E621Module : ModuleBase<ICommandContext>
    {
        private readonly E621Service _e621;

        public E621Module(E621Service e621)
        {
            _e621 = e621;
        }

        [Summary("search on e621")]
        [Command("e621")]
        [RequireNoGoodBoyMode]
        public async Task SearchRandom([Remainder] string tags)
        {
            var post = await _e621.SearchRandom(tags);
            await ReplyAsync(post.FileUrl);
        }
    }
}