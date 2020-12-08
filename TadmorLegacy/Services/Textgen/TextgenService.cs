﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MoreLinq;

namespace Tadmor.Services.Textgen
{
    [SingletonService]
    public class TextgenService
    {
        private readonly TextgenOptions _textgenOptions;
        private readonly Queue<string> _queuedEntries = new Queue<string>();
        private readonly SemaphoreSlim _generationSemaphore = new SemaphoreSlim(1, 1);

        public TextgenService(IOptionsSnapshot<TextgenOptions> textgenOptions)
        {
            _textgenOptions = textgenOptions.Value;
        }

        public async Task<string> Generate(double temperature, string? prompt = default)
        {
            if (!_queuedEntries.Any())
            {
                await GenerateEntries(temperature);
            }
            var entry = _queuedEntries.Dequeue();
            // if the buffer is starting to get small, refill it asynchronously
#pragma warning disable 4014
            GenerateEntries(temperature);
#pragma warning restore 4014
            return entry;
        }

        private async Task GenerateEntries(double temperature)
        {
            await _generationSemaphore.WaitAsync();
            try
            {
                if (_queuedEntries.Count < 256)
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "py",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        Arguments = $"\"{_textgenOptions.GeneratorPath}\" --temperature {temperature:0.00} --model_name logs",
                        WorkingDirectory = Path.GetDirectoryName(_textgenOptions.GeneratorPath),
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };
                    using var process = Process.Start(processInfo);
                    var tasks = await Task.WhenAll(
                        process.StandardError.ReadToEndAsync(),
                        process.StandardOutput.ReadToEndAsync());
                    var error = tasks[0];
                    var output = tasks[1];
                    process.Kill();
                    if (string.IsNullOrEmpty(output)) throw new InvalidOperationException(error);
                    const string endOftextDelimiter = "<|endoftext|>";
                    var entriesStartIndex = Math.Max(0, output.IndexOf(endOftextDelimiter));
                    var entries = output
                        .Substring(entriesStartIndex)
                        .Split(endOftextDelimiter, StringSplitOptions.RemoveEmptyEntries)[..^1]
                        .Select(e => e.Trim('\r', '\n', ' '))
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Distinct()
                        .Shuffle()
                        .ToList();
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
    }
}
