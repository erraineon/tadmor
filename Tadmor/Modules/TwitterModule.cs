using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Preconditions;
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

        [RequireWhitelist]
        [Summary("tweets a message or an image")]
        [Command("tweet")]
        public async Task Tweet([Remainder]string? input = null)
        {
            var image = await Context.Message.GetAllImagesAsync(Context.Client, new List<string>()).FirstOrDefaultAsync();
            await Tweet(Context.Message, input, image);
        }

        [RequireWhitelist]
        [Summary("tweets the user's last message")]
        [Command("tweet")]
        public async Task Tweet(IGuildUser user)
        {
            var lastMessage = await _activityMonitor.GetLastMessageAsync(user) ??
                throw new Exception($"{user.Mention} hasn't talked");
            var text = lastMessage.Resolve();
            var image = await lastMessage.GetAllImagesAsync(Context.Client, new List<string>()).FirstOrDefaultAsync();
            await Tweet(lastMessage, text, image);
        }

        private async Task Tweet(IMessage message, string? text, Services.Abstractions.Image? image)
        {
            var imageData = image != null ? await image.GetDataAsync() : null;
            var user = (IGuildUser) message.Author;
            var avatar = await user.GetAvatarAsync() ??
                         throw new Exception($"{user.Mention}'s avatar cannot be retrieved");
            var avatarData = await avatar.GetDataAsync();
            var toUpload = _imaging.Imitate(avatarData, user.Nickname ?? user.Username, text, imageData).ToArray();
            var tweetUrl = await _twitter.Tweet(toUpload);
            await ReplyAsync(tweetUrl);
        }
    }
}
