using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using MoreLinq;
using Tadmor.Search.Models;
using Tadmor.TextGeneration.Interfaces;
using Tadmor.TextGeneration.Models;

namespace Tadmor.TextGeneration.Services
{
    public class TadmorMindClient : ITadmorMindClient
    {
        private readonly TadmorMindOptions _tadmorMindOptions;
        private readonly Queue<string> _queuedEntries = new();
        private readonly SemaphoreSlim _generationSemaphore = new(1, 1);
        public TadmorMindClient(TadmorMindOptions tadmorMindOptions)
        {
            _tadmorMindOptions = tadmorMindOptions;
        }

        public async Task<string> GenerateAsync()
        {
            var generationTask = RefillBufferAndGenerateEntriesAsync();
            if (!_queuedEntries.Any()) await generationTask;
            var entry = _queuedEntries.Dequeue();
            return entry;
        }

        private async Task RefillBufferAndGenerateEntriesAsync()
        {
            await _generationSemaphore.WaitAsync();
            try
            {
                var bufferSize = _tadmorMindOptions.BufferSize ?? TadmorMindDefaults.BufferSize;
                if (_queuedEntries.Count < bufferSize)
                {
                    var entries = await GenerateEntriesAsync();
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

        private async Task<List<string>> GenerateEntriesAsync()
        {
            var text = await QueryGenerationAsync();
            const string eofDelimiter = "<|endoftext|>";
            var entriesStartIndex = Math.Max(0, text.IndexOf(eofDelimiter, StringComparison.Ordinal));
            var entries = text[entriesStartIndex..]
                .Split(eofDelimiter, StringSplitOptions.RemoveEmptyEntries)[..^1]
                .Select(e => e.Trim('\r', '\n', ' '))
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .Shuffle()
                .ToList();
            return entries;
        }

        private async Task<string> QueryGenerationAsync()
        {
            var requestBody = new {temperature = 1.0};
            var response = await _tadmorMindOptions.ServiceAddress
                .ConfigureRequest(s => s.Timeout = TimeSpan.FromMinutes(10))
                .PostJsonAsync(requestBody);
            var json = await response.GetJsonAsync();
            var text = json.text as string ??
                       throw new InvalidOperationException("no output was received from the text generator");
            return text;
        }
    }
}