using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Preconditions;
using Tadmor.Services.Sonagen;

namespace Tadmor.Modules
{
    [Summary("fursona generator")]
    [Group("sona")]
    [Name(nameof(SonagenModule))]
    public class SonagenModule : ModuleBase<ICommandContext>
    {
        private static readonly Random Random = new Random();
        private readonly SonagenService _sonagen;

        public SonagenModule(SonagenService sonagen)
        {
            _sonagen = sonagen;
        }

        private Task GenerateSona(Random random, IUser user = default, string seed = default)
        {
            var sona = _sonagen.GenerateSona(random);
            var builder = new EmbedBuilder();
            builder
                .WithDescription(sona.Description)
                .WithTitle($"{sona.Species} • {sona.Gender}");
            if (user != null) builder.WithAuthor(user);
            if (seed != null) builder.WithAuthor(seed);
            return ReplyAsync(string.Empty, embed: builder.Build());
        }

        [Summary("get your sona")]
        [Command]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        public Task GenerateUserSona()
        {
            return GenerateRandomSona((IGuildUser) Context.User);
        }

        [Summary("get a random sona")]
        [Command("random")]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        public Task GenerateRandomSona()
        {
            return GenerateSona(Random);
        }

        [Summary("get a sona for the specified name")]
        [Command]
        [Priority(-2)]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        public Task GenerateRandomSona([Remainder] string seed)
        {
            return GenerateSona(seed.ToRandom(), seed: seed);
        }

        [Summary("get a sona for the specified user")]
        [Command]
        [Priority(-1)]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        public async Task GenerateRandomSona(IGuildUser user)
        {
            var avatar = await user.GetAvatarAsync();
            var random = (user.Nickname, avatar.Id).ToRandom();
            await GenerateSona(random, user);
        }
    }
}