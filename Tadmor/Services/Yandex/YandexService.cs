using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Options;
using MoreLinq;
using YandexTranslateCSharpSdk;

namespace Tadmor.Services.Yandex
{
    public class YandexService
    {
        private readonly YandexTranslateSdk _yandex = new YandexTranslateSdk();
        private List<string> _languages;

        public YandexService(IOptions<YandexOptions> options)
        {
            _yandex.ApiKey = options.Value.ApiKey;
        }

        public async Task<string> BadTranslate(string input)
        {
            await EnsureLanguagesLoaded();
            var languagesChain = new[] {"en"}
                .Concat(new[] {"zh", "ja"}.Concat(_languages.RandomSubset(12)).Distinct())
                .Concat(new[] {"en"}).ToList();
            for (var index = 1; index < languagesChain.Count; index++)
            {
                var currentLanguage = languagesChain[index - 1];
                var nextLanguage = languagesChain[index];
                input = await _yandex.TranslateText(input, $"{currentLanguage}-{nextLanguage}");
            }

            return input;
        }

        private async Task EnsureLanguagesLoaded()
        {
            if (_languages == null) _languages = await _yandex.GetLanguages();
        }
    }

    public class YandexOptions
    {
        public string ApiKey { get; set; }
    }
}
