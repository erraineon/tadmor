using System;
using System.Linq;
using System.Text;
using Discord;
using E621;
using Humanizer;

namespace Tadmor.Extensions
{
    public static class DiscordExtensions
    {
        public static Random ToRandom(this IGuildUser user, 
            RandomDiscriminants discriminants = RandomDiscriminants.UserId)
        {
            bool HasFlag(RandomDiscriminants flag) => (discriminants & flag) != 0;

            var builder = new StringBuilder();
            if (HasFlag(RandomDiscriminants.UserId)) builder.Append(user.Id);
            if (HasFlag(RandomDiscriminants.GuildId)) builder.Append(user.GuildId);
            if (HasFlag(RandomDiscriminants.Nickname)) builder.Append(user.Nickname);
            if (HasFlag(RandomDiscriminants.AvatarId)) builder.Append(user.AvatarId);
            return builder.ToString().ToRandom();
        }

        public static Embed ToEmbed(this E621Post post)
        {
            var author = post.Artists != null ? post.Artists.Humanize() : post.Author;
            var builder = new EmbedBuilder()
                .WithTitle($"id: {post.Id} • score: {post.Score}")
                .WithAuthor(author)
                .WithDescription(post.Description)
                .WithImageUrl(post.FileUrl)
                .WithUrl($"https://e621.net/post/show/{post.Id}")
                .WithFooter(footer => footer.WithText(post.Tags["general"].Humanize().Truncate(2048)));
            return builder.Build();
        }
    }
}