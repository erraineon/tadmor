using System;
using System.Threading.Tasks;
using Discord.Commands;
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
            if (input.Length > 500) throw new Exception("no more than 500 characters");
            var badTranslation = await _yandex.BadTranslate(input);
            await ReplyAsync(badTranslation);
        }
    }
}