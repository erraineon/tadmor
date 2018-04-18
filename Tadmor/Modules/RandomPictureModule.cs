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
        public Task FoodGore() => PostRandomTumblrPicture("someoneatethis");

        [Command("fursuit")]
        public Task Fursuit() => PostRandomTumblrPicture("horrificfursuits");

        [Command("tumblr")]
        private async Task PostRandomTumblrPicture(string blogName)
        {
            var imageUrl = await _tumblr.GetRandomPost(blogName);
            var embed = new EmbedBuilder().WithImageUrl(imageUrl).Build();
            await ReplyAsync(string.Empty, embed: embed);
        }
    }
}