using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MoreLinq;
using YandexTranslateCSharpSdk;

namespace Tadmor.Services.Yandex
{
    [SingletonService]
    public class YandexService : IHostedService
    {
        private readonly bool _enabled;
        private readonly YandexTranslateSdk _yandex = new YandexTranslateSdk();
        private List<string>? _languages;

        public YandexService(IOptionsSnapshot<YandexOptions> options)
        {
            if (options.Value.ApiKey is {} apiKey)
            {
                _yandex.ApiKey = apiKey;
                _enabled = true;
            }
            else
            {
                _enabled = false;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_enabled) _languages = await _yandex.GetLanguages();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<string> BadTranslate(string input)
        {
            if (!_enabled) throw new Exception("yandex service is disabled. provide a yandex api key in the settings");
            var translation = await new[] {"en"}
                .Concat(new[] {"zh", "ja"})
                .Concat(_languages.RandomSubset(4))
                .Distinct()
                .Concat(new[] {"en"})
                .Pairwise((currentLanguage, nextLanguage) => $"{currentLanguage}-{nextLanguage}")
                .ToAsyncEnumerable()
                .AggregateAwaitAsync(
                    input,
                    (value, direction) => new ValueTask<string>(_yandex.TranslateText(value, direction)));

            return translation;
        }
    }
}