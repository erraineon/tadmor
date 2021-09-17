using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using MoreLinq;
using Tadmor.TextGeneration.Interfaces;
using Tadmor.TextGeneration.Models;

namespace Tadmor.TextGeneration.Services
{
    public class Gpt3TadmorMindClient : ITadmorMindClient
    {
        private const int MaxTokensToGenerate = 256;
        private readonly Gpt3TadmorMindOptions _gpt3TadmorMindOptions;

        public Gpt3TadmorMindClient(Gpt3TadmorMindOptions gpt3TadmorMindOptions)
        {
            _gpt3TadmorMindOptions = gpt3TadmorMindOptions;
        }

        public async Task<List<string>> GenerateEntriesAsync()
        {
            var text = await CompleteAsync(new
            {
                prompt = string.Empty,
                model = _gpt3TadmorMindOptions.ModelName,
                max_tokens = MaxTokensToGenerate,
            });
            var entryLimiters = new[] { "<|endoftext|>", "END" };
            var entriesStartIndex = Math.Max(0, entryLimiters.Min(e => text.IndexOf(e, StringComparison.Ordinal)));
            var entries = text[entriesStartIndex..]
                .Split(entryLimiters, StringSplitOptions.RemoveEmptyEntries)[..^1]
                .Select(RemoveWhiteSpace)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .Shuffle()
                .ToList();
            return entries;
        }

        public async Task<string> GenerateCompletionAsync(string prompt)
        {
            var completion = await CompleteAsync(new
            {
                prompt = prompt,
                model = _gpt3TadmorMindOptions.ModelName,
                max_tokens = MaxTokensToGenerate,
                temperature = 0.75f,
                stop = " END"
            });
            var separator = completion.FirstOrDefault() is var c && (char.IsLetterOrDigit(c) || c == '"')
                ? " "
                : string.Empty;
            return $"{prompt}{separator}{completion}";
        }

        private async Task<string> CompleteAsync(object requestData)
        {
            var result = await "https://api.openai.com/v1/completions"
                .WithOAuthBearerToken(_gpt3TadmorMindOptions.ApiKey)
                .PostJsonAsync(requestData); 
            var response = await result.GetJsonAsync();
            var text = response.choices[0].text.ToString() as string ?? throw new Exception("no data was generated");
            return RemoveWhiteSpace(text);
        }

        private static string RemoveWhiteSpace(string text)
        {
            return text.Replace("\n\n", "\n").Trim('\r', '\n', ' ');
        }
    }
}