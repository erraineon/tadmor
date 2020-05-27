using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Preconditions;
using Tadmor.Services.Yandex;

namespace Tadmor.Modules
{
    [Summary("bad translator")]
    public class BadTranslatorModule : ModuleBase<ICommandContext>
    {
        private readonly YandexService _yandex;

        public BadTranslatorModule(YandexService yandex)
        {
            _yandex = yandex;
        }

        [Summary("gives a bad translation of a message")]
        [Command("tr")]
        public async Task BadTranslate([Remainder]string input)
        {
            if (input.Length > 1000) throw new Exception("no more than 1000 characters");
            var badTranslation = await _yandex.BadTranslate(input);
            await ReplyAsync(badTranslation);
        }

        [Summary("gives a bad translation of a message")]
        [Command("tr")]
        [RequireReply]
        public async Task BadTranslate()
        {
            var input = await Context.GetQuotedContentAsync();
            await BadTranslate(input);
        }
    }
}