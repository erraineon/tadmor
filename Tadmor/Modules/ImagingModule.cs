using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        public ImagingModule(ImagingService imaging, ActivityMonitorService activityMonitor)
        {
            _imaging = imaging;
            _activityMonitor = activityMonitor;
        }

        [Summary("make a polygonal chart")]
        [Command("poly")]
        public async Task Poly(params string[] sides)
        {
            var rngAndAvatars = await GetRngAndAvatars().ToListAsync();
            var result = _imaging.Poly(rngAndAvatars, sides);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Summary("make a cartesian graph")]
        [Command("quad")]
        public async Task Quad(string top, string bottom, string left, string right)
        {
            var rngAndAvatars = await GetRngAndAvatars().ToListAsync();
            var result = _imaging.Quadrant(rngAndAvatars, top, bottom, left, right);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Summary("make a tier list")]
        [Command("tier")]
        public async Task Quad(params string[] tiers)
        {
            var rngAndAvatars = await GetRngAndAvatars().ToListAsync();
            var result = _imaging.Rank(rngAndAvatars, tiers);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Summary("make an alignment chart")]
        [Command("align")]
        public async Task AlignmentChart(params string[] options)
        {
            if (!options.Any()) options = new[] {"lawful", "neutral", "chaotic", "good", "neutral", "evil"};
            var rngAndAvatars = await GetRngAndAvatars().ToListAsync();
            var result = _imaging.AlignmentChart(rngAndAvatars, options);
            await Context.Channel.SendFileAsync(result, "result.png");
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
        public async Task Ok([ShowAsOptional] IGuildUser user, [Remainder] string? text = null)
        {
            if (text == null)
            {
                var lastMessage = await _activityMonitor.GetLastMessageAsync(user);
                text = (lastMessage as IUserMessage)?.Resolve() ?? throw new Exception($"{user.Mention} hasn't talked");
            }

            var avatarData = await user.GetAvatarAsync() is {} avatar ? await avatar.GetDataAsync() : null;
            if (avatarData == null) throw new Exception($"{user.Mention}'s avatar cannot be retrieved");
            var result = _imaging.Ok(text, avatarData);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Browsable(false)]
        [Command("ok")]
        public async Task Ok([Remainder] string text)
        {
            await Ok((IGuildUser) Context.User, text);
        }

        [Command("mimic")]
        public async Task Mimic([ShowAsOptional] IGuildUser user, [Remainder] string text)
        {
            if (text == null) text = await GetLastMessage(user);
            var avatarData = await user.GetAvatarAsync() is { } avatar ? await avatar.GetDataAsync() : null;
            if (avatarData == null) throw new Exception($"{user.Mention}'s avatar cannot be retrieved");
            var result = _imaging.Imitate(avatarData, user.Nickname ?? user.Username, text);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Browsable(false)]
        [Command("mimic")]
        public async Task Mimic([Remainder] string text)
        {
            await Mimic((IGuildUser) Context.User, text);
        }

        [Summary("fake sms with the specified text")]
        [Command("sms")]
        public async Task Sms([ShowAsOptional] IGuildUser user, [Remainder] string? text = null)
        {
            if (text == null) text = await GetLastMessage(user);
            var result = _imaging.Text(user.Nickname ?? user.Username, text);
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
            var text = (lastMessage as IUserMessage)?.Resolve() ?? throw new Exception($"{user.Mention} hasn't talked");
            return text;
        }

        private async Task UpDownGif(string text, IGuildUser? user, string direction)
        {
            var avatarData = user != null && await user.GetAvatarAsync() is {} avatar
                ? await avatar.GetDataAsync()
                : null;
            var result = _imaging.UpDownGif(text.ToUpper(), avatarData, $"{direction}.gif");
            await Context.Channel.SendFileAsync(result, "result.gif");
        }

        private async IAsyncEnumerable<(Random rng, byte[])> GetRngAndAvatars()
        {
            var rngAndAvatars = _activityMonitor.GetActiveUsers(Context.Guild)
                .SelectAwait(ImageRetrievalExtensions.GetAvatarAsync)
                .Where(image => image != null)
                .SelectAwait(async image => (image.Id.ToRandom(), await image.GetDataAsync()))
                .Reverse();
            await foreach (var rngAndAvatar in rngAndAvatars) yield return rngAndAvatar;
        }
    }
}