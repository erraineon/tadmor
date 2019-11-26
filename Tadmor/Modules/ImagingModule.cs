using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;
using Tadmor.Utils;

namespace Tadmor.Modules
{
    [Summary("images")]
    public class ImagingModule : ModuleBase<ICommandContext>
    {
        private readonly ActivityMonitorService _activityMonitor;
        private readonly ImagingService _imaging;
        private readonly ImagingServiceLegacy _imagingLegacy;

        public ImagingModule(ImagingServiceLegacy imagingLegacy, ActivityMonitorService activityMonitor, ImagingService imaging)
        {
            _imagingLegacy = imagingLegacy;
            _activityMonitor = activityMonitor;
            _imaging = imaging;
        }

        [Summary("make a polygonal chart")]
        [Command("poly")]
        public async Task Poly(params string[] sides)
        {
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _imagingLegacy.Poly(rngAndAvatars, sides);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Summary("make a cartesian graph")]
        [Command("quad")]
        public async Task Quad(string top, string bottom, string left, string right)
        {
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _imagingLegacy.Quadrant(rngAndAvatars, top, bottom, left, right);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Summary("make a tier list")]
        [Command("tier")]
        public async Task Tier(params string[] tiers)
        {
            var rngAndAvatars = await GetRngImages();
            var result = _imaging.Rank(rngAndAvatars, tiers);
            await Context.Channel.SendFileAsync(new MemoryStream(result), "result.png");
        }

        [Summary("make an alignment chart")]
        [Command("align")]
        public async Task AlignmentChart(params string[] options)
        {
            if (!options.Any()) options = new[] {"lawful", "neutral", "chaotic", "good", "neutral", "evil"};
            var rngImages = await GetRngImages();
            var result = _imaging.AlignmentChart(rngImages, options);
            await Context.Channel.SendFileAsync(new MemoryStream(result), "result.png");
        }

        [Browsable(false)]
        [Command("v")]
        public async Task Down([Remainder] string text)
        {
            await UpDownGif(text, default, "down");
        }

        [Browsable(false)]
        [Command("^")]
        public async Task Up([Remainder] string text)
        {
            await UpDownGif(text, default, "up");
        }

        [Summary("meme gif pointing down")]
        [Command("v")]
        public async Task Down([ShowAsOptional] IGuildUser user, [Remainder] string text = "")
        {
            await UpDownGif(text, user, "down");
        }

        [Summary("meme gif pointing up")]
        [Command("^")]
        public async Task Up([ShowAsOptional] IGuildUser user, [Remainder] string text = "")
        {
            await UpDownGif(text, user, "up");
        }

        [Summary("ok msg box with the specified text")]
        [Command("ok")]
        [Priority(1)]
        public async Task Ok([ShowAsOptional] IGuildUser user, [Remainder] string? text = null)
        {
            text ??= await GetLastMessage(user);
            var avatarData = await user.GetAvatarAsync() is {} avatar ? await avatar.GetDataAsync() : null;
            if (avatarData == null) throw new Exception($"{user.Mention}'s avatar cannot be retrieved");
            var result = _imagingLegacy.Ok(text, avatarData);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Browsable(false)]
        [Command("ok")]
        public async Task Ok([Remainder] string text)
        {
            await Ok((IGuildUser) Context.User, text);
        }

        [Summary("mimics someone else's message")]
        [Command("mimic")]
        public async Task Mimic(IGuildUser user, [Remainder] string? text = default)
        {
            var image = await Context.Message.GetAllImagesAsync(Context.Client, new List<string>()).FirstOrDefaultAsync();
            if (text == null && image == null) text = await GetLastMessage(user);
            var avatarData = await user.GetAvatarAsync() is { } avatar ? await avatar.GetDataAsync() : null;
            if (avatarData == null) throw new Exception($"{user.Mention}'s avatar cannot be retrieved");
            var imageData = image != null ? await image.GetDataAsync() : null;
            var result = _imagingLegacy.Imitate(avatarData, user.Nickname ?? user.Username, text, imageData);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Summary("mimics someone else's message after running a replacement on the text")]
        [Command("replace")]
        [Priority(1)]
        public async Task MimicReplace([ShowAsOptional] IGuildUser? user, string pattern, [Remainder] string replacement)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            var (author, text) = await Context.Channel.GetMessagesAsync()
                .Flatten()
                .Where(m => m.Id != Context.Message.Id && (user == null || m.Author.Id == user.Id))
                .Select(m => (message: m, newValue: regex.Replace(m.Content, replacement)))
                .Where(t => t.message.Content != t.newValue)
                .Select(t => (t.message.Author, t.newValue))
                .FirstAsync();
            await Mimic((IGuildUser) author, text);
        }

        [Summary("mimics someone else's message after running a replacement on the text")]
        [Command("replace")]
        [Browsable(false)]
        public async Task MimicReplace(string pattern, [Remainder] string replacement)
        {
            await MimicReplace(default, pattern, replacement);
        }

        [Summary("fake sms with the specified text")]
        [Command("sms")]
        public async Task Sms([ShowAsOptional] IGuildUser user, [Remainder] string? text = null)
        {
            if (text == null) text = await GetLastMessage(user);
            var result = _imagingLegacy.Text(user.Nickname ?? user.Username, text);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Browsable(false)]
        [Command("sms")]
        public async Task Sms([Remainder] string text)
        {
            await Sms((IGuildUser) Context.User, text);
        }

        private async Task<string> GetLastMessage(IGuildUser user)
        {
            var lastMessage = await _activityMonitor.GetLastMessageAsync(user);
            var text = lastMessage?.Resolve() ?? throw new Exception($"{user.Mention} hasn't talked");
            return text;
        }

        private async Task UpDownGif(string text, IGuildUser? user, string direction)
        {
            var avatarData = user != null && await user.GetAvatarAsync() is {} avatar
                ? await avatar.GetDataAsync()
                : null;
            var result = _imaging.UpDownGif(text.ToUpper(), avatarData, $"{direction}.gif");
            await Context.Channel.SendFileAsync(new MemoryStream(result), "result.gif");
        }

        [Obsolete]
        private async Task<List<(Random, byte[])>> GetRngAndAvatars()
        {
            var activeUsers = await _activityMonitor.GetActiveUsers(Context.Guild).ToListAsync();
            // as callers consume all avatars, it's better to request the images in parallel
            // rather than use IAsyncEnumerable
            var rngAndAvatars = (await Task.WhenAll(activeUsers
                    .Select(async user => await user.GetAvatarAsync() is { } avatar
                        ? (avatar.Id.ToRandom(), await avatar.GetDataAsync())
                        : default)))
                .Where(t => t != default)
                .ToList();
            return rngAndAvatars;
        }

        private async Task<List<RngImage>> GetRngImages()
        {
            var activeUsers = await _activityMonitor.GetActiveUsers(Context.Guild).ToListAsync();
            // as callers consume all avatars, it's better to request the images in parallel
            // rather than use IAsyncEnumerable
            var rngAndAvatars = (await Task.WhenAll(activeUsers
                    .Select(async user => await user.GetAvatarAsync() is { } avatar
                        ? new RngImage(await avatar.GetDataAsync(), avatar.Id.ToRandom())
                        : default)))
                .Where(t => t != default)
                .ToList();
            return rngAndAvatars;
        }
    }
}