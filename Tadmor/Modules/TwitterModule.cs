using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;
using Tadmor.Services.Twitter;

namespace Tadmor.Modules
{
    [Summary("twitter")]
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
        [Command("tweettext")]
        public async Task TweetText([Remainder]string input)
        {
            if (input.Length > 240) throw new Exception("no more than 240 characters");
            var tweetUrl = await _twitter.Tweet(input);
            await ReplyAsync(tweetUrl);
        }

        [Summary("tweets a message or an image")]
        [Command("tweet")]
        public async Task Tweet([Remainder]string? input = null)
        {
            await Tweet((IGuildUser)Context.User, input);
        }

        [Summary("tweets the user's last message")]
        [Command("tweet")]
        public async Task Tweet(IGuildUser user)
        {
            var lastMessage = await _activityMonitor.GetLastMessageAsync(user) ??
                throw new Exception($"{user.Mention} hasn't talked");
            var text = lastMessage.Resolve();
            await Tweet(user, text);
        }

        private async Task Tweet(IGuildUser user, string? text)
        {
            byte[] toUpload;
            if (text == null)
            {
                var image = await Context.GetAllImagesAsync(new List<string>(), true).FirstOrDefaultAsync() ?? 
                              throw new Exception("you must say something or post an image");
                toUpload = await image.GetDataAsync();
            }
            else
            {
                var avatar = await user.GetAvatarAsync() ??
                        throw new Exception($"{user.Mention}'s avatar cannot be retrieved");
                var avatarData = await avatar.GetDataAsync();
                toUpload = _imaging.Imitate(avatarData, user.Nickname ?? user.Username, text).ToArray();
            }

            var tweetUrl = await _twitter.Tweet(toUpload);
            await ReplyAsync(tweetUrl);
        }
    }
}
