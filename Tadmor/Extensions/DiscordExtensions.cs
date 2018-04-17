using System.Linq;
using Discord;
using E621;
using Humanizer;

namespace Tadmor.Extensions
{
    public static class DiscordExtensions
    {
        public static Embed ToEmbed(this E621Post post)
        {
            var author = post.Artists.Any() ? post.Artists.Humanize() : post.Author;
            var builder = new EmbedBuilder()
                .WithTitle($"id: {post.Id} • score: {post.Score}")
                .WithAuthor(author)
                .WithDescription(post.Description.Truncate(EmbedBuilder.MaxDescriptionLength))
                .WithImageUrl(post.FileUrl)
                .WithUrl($"https://e621.net/post/show/{post.Id}");
            return builder.Build();
        }
    }
}