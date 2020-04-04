using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Reddit;

namespace Tadmor.Modules
{
    [Summary("reddit time")]
    public class UpvoteModule : ModuleBase<ICommandContext>
    {
        private readonly RedditService _reddit;

        public UpvoteModule(RedditService reddit)
        {
            _reddit = reddit;
        }

        [Summary("upvotes a user's message")]
        [Command("upvote")]
        public async Task Upvote(IGuildUser? user = null)
        {
            var currentUserId = Context.User.Id;
            if (user?.Id == currentUserId) throw new Exception("you can't upvote yourself");
            var target = await Context.Channel.GetMessagesAsync()
                .Flatten()
                .Where(m => m.Id != Context.Message.Id &&
                            (user == null || m.Author.Id == user.Id && m.Author.Id != currentUserId))
                .Select(m => m.Author)
                .FirstOrDefaultAsync() as IGuildUser;
            if (target == null) throw new Exception($"{user?.Nickname} hasn't posted recently");
            var totalUpvotes = await _reddit.Upvote(target);
            await ReplyAsync(
                $"you upvoted {target.Nickname}'s post. they have received a total of {totalUpvotes} upvotes");
        }
        [Summary("shows upvotes on the current guild")]
        [Command("upvotes")]
        public async Task Upvotes()
        {
            var upvotes = await _reddit.GetUpvotes(Context.Guild.Id);
            var upvoteStrings = await Task.WhenAll(upvotes
                .OrderByDescending(m => m.UpvotesCount)
                .Select(GetStringDescription));
            await ReplyAsync(upvoteStrings.Any()
                ? string.Join(Environment.NewLine, upvoteStrings)
                : "no upvotes were given");
        }

        private async Task<string> GetStringDescription(Upvote upvote)
        {
            var user = await Context.Guild.GetUserAsync(upvote.UserId);
            return $"{user.Nickname} has {upvote.UpvotesCount} upvotes";
        }
    }
}