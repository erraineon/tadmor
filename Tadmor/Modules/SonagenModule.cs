using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Services.Sonagen;

namespace Tadmor.Modules
{
    [Group("sona")]
    public class SonagenModule : ModuleBase<SocketCommandContext>
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
            var fields = sona.AttributesByGroup
                .OrderBy(group => group.Key)
                .Select(group => new EmbedFieldBuilder()
                    .WithName(group.Key)
                    .WithValue(string.Join(Environment.NewLine, group.Select(a => a.value))));
            var builder = new EmbedBuilder();
            builder
                .WithDescription(sona.Description)
                .WithTitle($"{sona.Species} • {sona.Gender}");
            if (user != null) builder.WithAuthor(user);
            if (seed != null) builder.WithAuthor(seed);
            builder.Fields.AddRange(fields);
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
            var random = user.ToRandom(RandomDiscriminants.UserId | RandomDiscriminants.Nickname);
            return GenerateSona(random, user);
        }
    }
}