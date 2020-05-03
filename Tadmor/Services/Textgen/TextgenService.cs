using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Tadmor.Services.Textgen
{
    [SingletonService]
    public class TextgenService
    {
        private readonly TextgenOptions _textgenOptions;

        public TextgenService(IOptionsSnapshot<TextgenOptions> textgenOptions)
        {
            _textgenOptions = textgenOptions.Value;
        }

        public async Task<string> Generate(double temperature, string? prompt = default)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "py",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                Arguments =
                    $"\"{_textgenOptions.GeneratorPath}\" --temperature {temperature:0.00} --model_name logs",
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
            const string endOftextDelimiter = "<|endoftext|>";
            if (output.StartsWith(endOftextDelimiter)) output = output.Substring(endOftextDelimiter.Length);
            var firstEndOfText = output.IndexOf(endOftextDelimiter);
            if (firstEndOfText >= 0) output = output.Substring(0, firstEndOfText);
            output = output.Trim('\r', '\n', ' ');
            if (string.IsNullOrEmpty(output)) throw new InvalidOperationException(error);
            return output;
        }
    }
}
