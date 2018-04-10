using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MoreLinq;
using Tadmor.Extensions;
using Tadmor.Services.Imaging;

namespace Tadmor.Modules
{
    public class ImagingModule : ModuleBase<SocketCommandContext>
    {
        private readonly ImagingService _service;
        private readonly HttpClient _client;

        public ImagingModule(ImagingService service, HttpClient client)
        {
            _service = service;
            _client = client;
        }

        [Command("mcd")]
        public async Task McDonalds()
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
                .ToList());
            var result = _service.McDonalds(rngAndAvatars);
            await Context.Channel.SendFileAsync(result, "result.png");
        }
    }
}