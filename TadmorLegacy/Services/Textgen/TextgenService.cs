using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MoreLinq;
using Newtonsoft.Json.Linq;

namespace Tadmor.Services.Textgen
{
    [SingletonService]
    public class TextgenService
    {
        private readonly TextgenOptions _textgenOptions;
        private readonly Queue<string> _queuedEntries = new Queue<string>();
        private readonly SemaphoreSlim _generationSemaphore = new SemaphoreSlim(1, 1);
        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3000)
        };

        public TextgenService(IOptionsSnapshot<TextgenOptions> textgenOptions)
        {
            _textgenOptions = textgenOptions.Value;
        }

        public async Task<string> Generate(double temperature, string? prompt = default)
        {
            if (!_queuedEntries.Any())
            {
                await GenerateEntriesIfNeeded(temperature);
            }
            var entry = _queuedEntries.Dequeue();
            // if the buffer is starting to get small, refill it asynchronously
#pragma warning disable 4014
            GenerateEntriesIfNeeded(temperature);
#pragma warning restore 4014
            return entry;
        }

        private async Task GenerateEntriesIfNeeded(double temperature)
        {
            await _generationSemaphore.WaitAsync();
            try
            {
                if (_queuedEntries.Count < 256)
                {
                    var entries = await GenerateEntries(temperature);
                    foreach (var entry in entries)
                    {
                        _queuedEntries.Enqueue(entry);
                    }
                }
            }
            finally
            {
                _generationSemaphore.Release();
            }
        }

        private async Task<List<string>> GenerateEntries(double temperature)
        {
            var content = new StringContent("{\"temperature\": 1.0}", Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(_textgenOptions.GeneratorPath, content);
            var json = await response.Content.ReadAsStringAsync();
            var jobject = JObject.Parse(json);
            var output = (string)jobject["text"];
            if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("no output was received from the text generator");
            const string endOftextDelimiter = "<|endoftext|>";
            var entriesStartIndex = Math.Max(0, output.IndexOf(endOftextDelimiter, StringComparison.Ordinal));
            var entries = output
                .Substring(entriesStartIndex)
                .Split(endOftextDelimiter, StringSplitOptions.RemoveEmptyEntries)[..^1]
                .Select(e => e.Trim('\r', '\n', ' '))
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .Shuffle()
                .ToList();
            return entries;
        }
    }
}
