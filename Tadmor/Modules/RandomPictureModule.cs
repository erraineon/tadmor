using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Tumblr;

namespace Tadmor.Modules
{
    public class RandomPictureModule : ModuleBase<SocketCommandContext>
    {
        private readonly TumblrService _tumblr;

        public RandomPictureModule(TumblrService tumblr)
        {
            _tumblr = tumblr;
        }

        [Command("foodgore")]
        public async Task FoodGore()
        {
            var imageUrl = await _tumblr.GetRandomPost("someoneatethis");
            var embed = new EmbedBuilder().WithImageUrl(imageUrl).Build();
            await ReplyAsync(string.Empty, embed: embed);
        }
    }
}