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
            var message = await GetVotedMessage(user);
            await Vote(message, VoteType.Upvote);
        }

        [Summary("upvotes a user's message")]
        [Command("upvote")]
        [RequireReply]
        [Priority(1)]
        public async Task Upvote()
        {
            var quotedMessage = await GetVotedMessage();
            await Vote(quotedMessage, VoteType.Upvote);
        }


        [Summary("downvotes a user's message")]
        [Command("downvote")]
        public async Task Downvote(IGuildUser? user = null)
        {
            var message = await GetVotedMessage(user);
            await Vote(message, VoteType.Downvote);
        }

        [Summary("downvotes a user's message")]
        [Command("downvote")]
        [RequireReply]
        [Priority(1)]
        public async Task Downvote()
        {
            var quotedMessage = await GetVotedMessage();
            await Vote(quotedMessage, VoteType.Downvote);
        }

        private async Task<IMessage> GetVotedMessage()
        {
            var quotedMessage = await Context.GetQuotedMessageAsync();
            if (quotedMessage.Author.Id == Context.User.Id) throw new Exception("you can't vote yourself");
            return quotedMessage;
        }

        private async Task Vote(IMessage message, VoteType voteType)
        {
            var targetUser = message.Author as IGuildUser ??
                throw new Exception("the message's author could not be retrieved");
            var totalUpvotes = await _reddit.Vote(message, targetUser, Context.User, voteType);
            var verb = voteType == VoteType.Upvote ? "upvoted" : "downvoted";
            await ReplyAsync($"you {verb} {targetUser.Nickname}'s post. they have a user score of {totalUpvotes}");
        }

        [Summary("shows upvotes on the current guild")]
        [Command("upvotes")]
        public async Task Upvotes()
        {
            var upvotes = await _reddit.GetUpvoteCounts(Context.Guild.Id);
            var upvoteStrings = await Task.WhenAll(upvotes
                .OrderByDescending(kvp => kvp.Value.upvoteCount - kvp.Value.downvoteCount)
                .Select(GetStringDescription));
            await ReplyAsync(upvoteStrings.Any()
                ? string.Join(Environment.NewLine, upvoteStrings)
                : "no upvotes were given");
        }

        private async Task<string> GetStringDescription(KeyValuePair<ulong, (int upvoteCount, int downvoteCount)> voteCount)
        {
            var (upvoteCount, downvoteCount) = voteCount.Value;
            var userScore = upvoteCount - downvoteCount;
            var user = await Context.Guild.GetUserAsync(voteCount.Key);
            return $"{user.Nickname} has {upvoteCount} upvotes and {downvoteCount} downvotes for a score of {userScore}";
        }

        private async Task<IMessage> GetVotedMessage(IGuildUser? user)
        {
            var currentUserId = Context.User.Id;
            if (user?.Id == currentUserId) throw new Exception("you can't vote yourself");
            var message = await Context.Channel.GetMessagesAsync()
                .Flatten()
                .Where(m => m.Id != Context.Message.Id &&
                    (user == null || m.Author.Id == user.Id && m.Author.Id != currentUserId))
                .FirstOrDefaultAsync();
            if (message == null) throw new Exception($"{user?.Nickname} hasn't posted recently");
            return message;
        }
    }
}