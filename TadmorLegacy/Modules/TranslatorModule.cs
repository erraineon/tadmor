using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Preconditions;
using Tadmor.Services.Translation;

namespace Tadmor.Modules
{
    [Summary("translator")]
    public class TranslatorModule : ModuleBase<ICommandContext>
    {
        private readonly NoopTranslationService _translationService;

        public TranslatorModule(NoopTranslationService translationService)
        {
            _translationService = translationService;
        }

        [Summary("gives a bad translation of a message")]
        [Command("badtr")]
        public async Task BadTranslate([Remainder]string input)
        {
            if (input.Length > 1000) throw new Exception("no more than 1000 characters");
            var badTranslation = await _translationService.BadTranslate(input);
            await ReplyAsync(badTranslation);
        }

        [Summary("gives a bad translation of a message")]
        [Command("badtr")]
        [RequireReply]
        public async Task BadTranslate()
        {
            var input = await Context.GetQuotedContentAsync();
            await BadTranslate(input);
        }

        [Summary("translates a message to the specified language")]
        [Command("tr")]
        public async Task Translate(string language, [Remainder]string input)
        {
            if (input.Length > 1000) throw new Exception("no more than 1000 characters");
            var translation = await _translationService.DetectAndTranslate(input, language);
            await ReplyAsync(translation);
        }

        [Summary("gives a bad translation of a message")]
        [Command("tr")]
        [RequireReply]
        public async Task Translate(string language)
        {
            var input = await Context.GetQuotedContentAsync();
            await Translate(language, input);
        }
    }
}