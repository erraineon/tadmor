using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;
using Tadmor.Services.Twitter;

namespace Tadmor.Modules
{
    public class TwitterModule : ModuleBase<ICommandContext>
    {
        private readonly TwitterService _twitter;
        private readonly ActivityMonitorService _activityMonitor;
        private readonly ImagingService _imaging;

        public TwitterModule(TwitterService twitter, ActivityMonitorService activityMonitor, ImagingService imaging)
        {
            _twitter = twitter;
            _activityMonitor = activityMonitor;
            _imaging = imaging;
        }

        [RequireOwner]
        [Summary("tweets a message")]
        [Command("tweet")]
        public async Task Tweet([Remainder]string input)
        {
            if (input.Length > 240) throw new Exception("no more than 240 characters");
            var tweetUrl = await _twitter.Tweet(input);
            await ReplyAsync(tweetUrl);
        }

        [Summary("tweets a message")]
        [Command("tweet")]
        public async Task Tweet(IGuildUser user)
        {
            var lastMessage = await _activityMonitor.GetLastMessageAsync(user) ??
                throw new Exception($"{user.Mention} hasn't talked");
            var text = lastMessage.Resolve();
            var avatarData = await user.GetAvatarAsync() is { } avatar ? await avatar.GetDataAsync() : null;
            if (avatarData == null) throw new Exception($"{user.Mention}'s avatar cannot be retrieved");
            var result = _imaging.Imitate(avatarData, user.Nickname ?? user.Username, text);
            var tweetUrl = await _twitter.Tweet(result);
            await ReplyAsync(tweetUrl);
        }
    }
}
