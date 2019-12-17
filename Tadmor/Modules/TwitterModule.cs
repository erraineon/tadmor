using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Preconditions;
using Tadmor.Services.Imaging;
using Tadmor.Services.Twitter;
using Tadmor.Utils;
using Image = Tadmor.Services.Abstractions.Image;

namespace Tadmor.Modules
{
    [Summary("twitter")]
    public class TwitterModule : ModuleBase<ICommandContext>
    {
        private readonly TwitterService _twitter;
        private readonly ImagingService _imaging;

        public TwitterModule(TwitterService twitter, ImagingService imaging)
        {
            _twitter = twitter;
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
            if (image == null && input == null) throw new Exception("provide either text or an image");
            var imitation = await Imitate(image, (IGuildUser) Context.User, input);
            await Tweet(imitation);
        }

        [RequireWhitelist]
        [Summary("tweets the user's last message")]
        [Command("tweet")]
        public async Task Tweet(IGuildUser user)
        {
            await Tweet(user, 1);
        }

        [RequireWhitelist]
        [Summary("tweets the specified amount of messages ending with the last user's message")]
        [Command("tweet")]
        [Priority(1)]
        public async Task Tweet([ShowAsOptional]IGuildUser? user, int messagesCount)
        {
            if (messagesCount > 16) throw new Exception("please no");
            var messages = await Context.Channel.GetMessagesAsync()
                .Flatten()
                .OfType<IUserMessage>()
                .SkipWhile(m => m.Id == Context.Message.Id || user != null && m.Author.Id != user.Id)
                .Take(messagesCount)
                .ToListAsync();
            if (!messages.Any())
                throw new Exception(user != null
                    ? $"{user.Mention} hasn't talked"
                    : "there's no messages in this chat");
            var images = await Task.WhenAll(messages.Select(async message =>
            {
                var image = await message.GetAllImagesAsync(Context.Client, new List<string>()).FirstOrDefaultAsync();
                var messageAuthor = message.Author;
                var text = message.Resolve();
                return await Imitate(image, (IGuildUser) messageAuthor, text);
            }));
            var stackedImages = _imaging.StackVertically(images.Reverse(), 5, 10, default);
            await Tweet(stackedImages);
        }

        [RequireWhitelist]
        [Summary("tweets the specified amount of messages ending with the last user's message")]
        [Command("tweet")]
        [Priority(1)]
        [Browsable(false)]
        public async Task Tweet(int messagesCount)
        {
            await Tweet(null, messagesCount);
        }

        private async Task Tweet(byte[] image)
        {
            var tweetUrl = await _twitter.Tweet(image);
            await ReplyAsync(tweetUrl);
        }

        private async Task<byte[]> Imitate(Image? image, IGuildUser user, string? text)
        {
            var imageData = image != null ? await image.GetDataAsync() : null;
            var avatar = await user.GetAvatarAsync() ?? throw new Exception($"{user.Mention}'s avatar cannot be retrieved");
            var avatarData = await avatar.GetDataAsync();
            var imitation = _imaging.Imitate(avatarData, user.Nickname ?? user.Username, text, imageData);
            return imitation.ToArray();
        }
    }
}
