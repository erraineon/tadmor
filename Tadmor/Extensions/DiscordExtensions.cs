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

            //.net core adds randomness to hash code generation. use this for fixed behavior
            int GetStableHashCode(string str)
            {
                unchecked
                {
                    var hash1 = 5381;
                    var hash2 = hash1;

                    for (var i = 0; i < str.Length && str[i] != '\0'; i += 2)
                    {
                        hash1 = ((hash1 << 5) + hash1) ^ str[i];
                        if (i == str.Length - 1 || str[i + 1] == '\0')
                            break;
                        hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                    }

                    return hash1 + hash2 * 1566083941;
                }
            }

            var builder = new StringBuilder();
            if (HasFlag(RandomDiscriminants.UserId)) builder.Append(user.Id);
            if (HasFlag(RandomDiscriminants.GuildId)) builder.Append(user.GuildId);
            if (HasFlag(RandomDiscriminants.Nickname)) builder.Append(user.Nickname);
            if (HasFlag(RandomDiscriminants.AvatarId)) builder.Append(user.AvatarId);
            var hashCode = GetStableHashCode(builder.ToString());
            return new Random(hashCode);
        }

        public static Embed ToEmbed(this E621Post post)
        {
            var author = post.Artists.Any() ? post.Artists.Humanize() : post.Author;
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