using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Extensions;
using Tadmor.Preconditions;
using Tadmor.Services.Options;
using Tadmor.Services.Textgen;
using Tadmor.Services.Yandex;

namespace Tadmor.Modules
{
    [Summary("text generation")]
    public class TextgenModule : ModuleBase<ICommandContext>
    {
        private readonly TextgenService _textgen;
        private readonly ChatOptionsService _chatOptions;
        private readonly YandexService _yandex;

        public TextgenModule(TextgenService textgen, ChatOptionsService chatOptions, YandexService yandex)
        {
            _textgen = textgen;
            _chatOptions = chatOptions;
            _yandex = yandex;
        }

        [RequireWhitelist]
        [Summary("generates text based on a trained model")]
        [Command("gen")]
        public async Task Generate([Remainder] double? temperature = null)
        {
            var text = await _textgen.Generate(1);
            var options = _chatOptions.GetOptions();
            var guildOptions = _chatOptions.GetGuildOptions(Context.Guild.Id, options.Value);
            if (!string.IsNullOrEmpty(guildOptions.AutoTranslateLanguage))
            {
                text = await _yandex.Translate(text, $"en-{guildOptions.AutoTranslateLanguage}");
            }
            await Context.Channel.SendMessageAsync(text);
        }
    }
}