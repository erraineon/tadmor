using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Sonagen;

namespace Tadmor.Modules
{
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

        [Command]
        public Task GenerateUserSona()
        {
            return GenerateRandomSona((IGuildUser) Context.User);
        }

        [Command("random")]
        public Task GenerateRandomSona()
        {
            return GenerateSona(Random);
        }

        [Command]
        [Priority(-2)]
        public Task GenerateRandomSona([Remainder] string seed)
        {
            return GenerateSona(seed.ToRandom(), seed: seed);
        }
        [Command]
        [Priority(-1)]
        public Task GenerateRandomSona(IGuildUser user)
        {
            var random = (user.Nickname, user.AvatarId).ToRandom();
            return GenerateSona(random, user);
        }
    }
}