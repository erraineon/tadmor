using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;

namespace Tadmor.Modules
{
    public class ImagingModule : ModuleBase<SocketCommandContext>
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly ActivityMonitorService _activityMonitor;
        private readonly ImagingService _imaging;

        public ImagingModule(ImagingService imaging, ActivityMonitorService activityMonitor)
        {
            _imaging = imaging;
            _activityMonitor = activityMonitor;
        }

        [Command("tri")]
        public async Task Triangle(string opt1, string opt2, string opt3, [Remainder] string title = "")
        {
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _imaging.Triangle(rngAndAvatars, opt1, opt2, opt3, title);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Command("quad")]
        public async Task Quadrant(string opt1, string opt2, string opt3, string opt4)
        {
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _imaging.Quadrant(rngAndAvatars, opt1, opt2, opt3, opt4);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Command("mcd")]
        public Task McDonalds()
        {
            const string opt1 = "\"We have food at home\"";
            const string opt2 = "*Pulls into the drive through as chilren cheer*\n" +
                                "*Orders a single black coffee and leaves*";
            const string opt3 = "\"MCDONALDS!\nMCDONALDS! MCDONALDS!\"";
            return Triangle(opt1, opt2, opt3, "CHILDREN YELLING: MCDONALDS! MCDONALDS! MCDONALDS!");
        }

        [Command("align")]
        public async Task AlignmentChart(params string[] options)
        {
            if (!options.Any()) options = new[] {"lawful", "neutral", "chaotic", "good", "neutral", "evil"};
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _imaging.AlignmentChart(rngAndAvatars, options);
            await Context.Channel.SendFileAsync(result, "result.png");
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