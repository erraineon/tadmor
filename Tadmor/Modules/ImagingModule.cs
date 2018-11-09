using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;

namespace Tadmor.Modules
{
    [Summary("images")]
    public class ImagingModule : ModuleBase<ICommandContext>
    {
        private static readonly HttpClient Client = new HttpClient();
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
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _imaging.Poly(rngAndAvatars, sides);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Summary("make a cartesian graph")]
        [Command("quad")]
        public async Task Quad(string top, string bottom, string left, string right)
        {
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _imaging.Quadrant(rngAndAvatars, top, bottom, left, right);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Summary("make an alignment chart")]
        [Command("align")]
        public async Task AlignmentChart(params string[] options)
        {
            if (!options.Any()) options = new[] {"lawful", "neutral", "chaotic", "good", "neutral", "evil"};
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _imaging.AlignmentChart(rngAndAvatars, options);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Browsable(false)]
        [Command("v")]
        public async Task Down([Remainder]string text)
        {
            await UpDownGif(text, default, "down");
        }

        [Browsable(false)]
        [Command("^")]
        public async Task Up([Remainder]string text)
        {
            await UpDownGif(text, default, "up");
        }
        [Summary("meme gif pointing down")]
        [Command("v")]
        public async Task Down(IGuildUser user = default, [Remainder]string text = "")
        {
            await UpDownGif(text, user, "down");
        }

        [Summary("meme gif pointing up")]
        [Command("^")]
        public async Task Up(IGuildUser user = default, [Remainder]string text = "")
        {
            await UpDownGif(text, user, "up");
        }

        [Summary("ok msg box with the specified text")]
        [Command("ok")]
        public async Task Ok(IGuildUser user = default, [Remainder]string text = null)
        {
            if (text == null)
            {
                var lastMessage = await _activityMonitor.GetLastMessage(user);
                text = (lastMessage as IUserMessage)?.Resolve() ?? throw new Exception($"{user.Mention} hasn't talked");
            }
            var avatar = await Client.GetByteArrayAsync(user.GetAvatarUrl());
            var result =  _imaging.Ok(text, avatar);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Browsable(false)]
        [Command("ok")]
        public async Task Ok([Remainder]string text)
        {
            if (Context.User is IGuildUser guildUser) await Ok(guildUser, text);
        }

        [Summary("fake sms with the specified text")]
        [Command("sms")]
        public async Task Text(IGuildUser user = default, [Remainder]string text = null)
        {
            if (user == default) user = (IGuildUser) Context.User;
            if (text == null) text = await GetLastMessage(user);
            var result =  _imaging.Text(user.Nickname ?? user.Username, text);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Browsable(false)]
        [Command("sms")]
        public async Task Text([Remainder]string text)
        {
            if (Context.User is IGuildUser guildUser) await Text(guildUser, text);
        }

        private async Task<string> GetLastMessage(IGuildUser user)
        {
            var lastMessage = await _activityMonitor.GetLastMessage(user);
            var text = (lastMessage as IUserMessage)?.Resolve() ?? throw new Exception($"{user.Mention} hasn't talked");
            return text;
        }

        private async Task UpDownGif(string text, IUser user, string direction)
        {
            var avatar = user == default ? null : await Client.GetByteArrayAsync(user.GetAvatarUrl());
            var result = _imaging.UpDownGif(text.ToUpper(), avatar, $"{direction}.gif");
            await Context.Channel.SendFileAsync(result, "result.gif");
        }

        private async Task<(Random rng, byte[])[]> GetRngAndAvatars()
        {
            var users = await _activityMonitor.GetActiveUsers(Context.Guild);
            var rngAndAvatars = await Task.WhenAll((from user in users
                    let avatarUrl = user.GetAvatarUrl()
                    where avatarUrl != null
                    let avatarTask = Client.GetByteArrayAsync(avatarUrl)
                    select (rng: user.AvatarId.ToRandom(), avatarTask))
                .Select(async tuple => (tuple.rng, await tuple.avatarTask))
                .Reverse()
                .ToList());
            return rngAndAvatars;
        }
    }
}