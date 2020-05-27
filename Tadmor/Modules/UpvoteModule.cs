using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Preconditions;
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
            var message = await Context.Channel.GetMessagesAsync()
                .Flatten()
                .Where(m => m.Id != Context.Message.Id &&
                            (user == null || m.Author.Id == user.Id && m.Author.Id != currentUserId))
                .FirstOrDefaultAsync();
            if (message == null) throw new Exception($"{user?.Nickname} hasn't posted recently");
            await Upvote(message);
        }

        [Summary("upvotes a user's message")]
        [Command("upvote")]
        [RequireReply]
        [Priority(1)]
        public async Task Upvote()
        {
            var quotedMessage = await Context.GetQuotedMessageAsync();
            if (quotedMessage.Author.Id == Context.User.Id) throw new Exception("you can't upvote yourself");
            await Upvote(quotedMessage);
        }

        private async Task Upvote(IMessage message)
        {
            var targetUser = message.Author as IGuildUser ??
                throw new Exception("the message's author could not be retrieved");
            var totalUpvotes = await _reddit.Upvote(message, targetUser, Context.User);
            await ReplyAsync(
                $"you upvoted {targetUser.Nickname}'s post. they have received a total of {totalUpvotes} upvotes");
        }

        [Summary("shows upvotes on the current guild")]
        [Command("upvotes")]
        public async Task Upvotes()
        {
            var upvotes = await _reddit.GetUpvoteCounts(Context.Guild.Id);
            var upvoteStrings = await Task.WhenAll(upvotes
                .OrderByDescending(kvp => kvp.Value)
                .Select(GetStringDescription));
            await ReplyAsync(upvoteStrings.Any()
                ? string.Join(Environment.NewLine, upvoteStrings)
                : "no upvotes were given");
        }

        private async Task<string> GetStringDescription(KeyValuePair<ulong, int> upvoteCount)
        {
            var user = await Context.Guild.GetUserAsync(upvoteCount.Key);
            return $"{user.Nickname} has {upvoteCount.Value} upvotes";
        }
    }
}