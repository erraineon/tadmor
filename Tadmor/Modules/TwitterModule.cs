using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Services.Data;
using Tadmor.Services.Twitter;

namespace Tadmor.Modules
{

    public class TwitterModule : ModuleBase<ICommandContext>
    {
        private readonly TwitterService _twitter;
        private readonly AppDbContext _context;

        public TwitterModule(TwitterService twitter, AppDbContext context)
        {
            _twitter = twitter;
            _context = context;
        }

        [Command("twitter")]
        public async Task RandomMedia(string username)
        {
            var statusUrl = await _twitter.GetRandomMediaStatusUrl(_context, username);
            await ReplyAsync(statusUrl);
        }

    }
}
