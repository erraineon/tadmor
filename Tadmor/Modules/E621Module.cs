using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using E621;
using Humanizer;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Tadmor.Preconditions;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Commands;
using Tadmor.Services.E621;

namespace Tadmor.Modules
{
    [Summary("e621")]
    public class E621Module : ModuleBase<ICommandContext>
    {
        private readonly E621Service _e621;

        public E621Module(E621Service e621)
        {
            _e621 = e621;
        }

        [Summary("search on e621")]
        [Command("e621")]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        public async Task SearchRandom([Remainder] string tags)
        {
            var post = await _e621.SearchRandom(tags);
            await ReplyAsync(post.File.Url);
        }

        [Summary("starts a pokemon recognition game")]
        [Command("pokemongame")]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        [RequireWhitelist]
        public async Task PokemonGame()
        {
            await _e621.ToggleSession(Context.Channel);
        }
    }
}