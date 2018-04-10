using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;

namespace Tadmor.Modules
{
    public class ImagingModule : ModuleBase<SocketCommandContext>
    {
        private readonly HttpClient _client;
        private readonly ImagingService _service;

        public ImagingModule(ImagingService service, HttpClient client)
        {
            _service = service;
            _client = client;
        }

        [Command("tri")]
        public async Task Triangle(string opt1, string opt2, string opt3, [Remainder] string title = "")
        {
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _service.Triangle(rngAndAvatars, opt1, opt2, opt3, title);
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
        public async Task AlignmentChart(string ea1, string ma, string ea2, string eb1, string mb, string eb2)
        {
            var rngAndAvatars = await GetRngAndAvatars();
            var result = _service.AlignmentChart(rngAndAvatars, ea1, ma, ea2, eb1, mb, eb2);
            await Context.Channel.SendFileAsync(result, "result.png");
        }

        [Command("align")]
        public Task AlignmentChart()
        {
            return AlignmentChart("lawful", "neutral", "chaotic", "good", "neutral", "evil");
        }

        private async Task<(Random rng, byte[])[]> GetRngAndAvatars()
        {
            var users = Context.Channel.GetMessagesAsync()
                .Flatten()
                .Select(message => message.Author as IGuildUser)
                .Where(user => user != null)
                .Distinct();
            var rngAndAvatars = await Task.WhenAll(await (from user in users
                    let avatarUrl = user.GetAvatarUrl()
                    where avatarUrl != null
                    let avatarTask = _client.GetByteArrayAsync(avatarUrl)
                    select (rng: user.ToRandom(RandomDiscriminants.AvatarId), avatarTask))
                .Select(async tuple => (tuple.rng, await tuple.avatarTask))
                .Reverse()
                .ToList());
            return rngAndAvatars;
        }
    }
}