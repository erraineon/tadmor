using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Services.E621;

namespace Tadmor.Modules
{
    public class E621Module : ModuleBase<ICommandContext>
    {
        private readonly E621Service _e621;

        public E621Module(E621Service e621)
        {
            _e621 = e621;
        }

        [Command("e621")]
        public async Task SearchRandom([Remainder] string tags)
        {
            var post = await _e621.SearchRandom(tags);
            await ReplyAsync(post.FileUrl);
        }
    }
}