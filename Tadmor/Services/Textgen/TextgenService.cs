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
                    $"\"{_textgenOptions.GeneratorPath}\" {temperature}{(prompt != null ? $" \"{prompt}\"" : default)}",
                WorkingDirectory = Path.GetDirectoryName(_textgenOptions.GeneratorPath),
                CreateNoWindow = true,
            };
            using var process = Process.Start(processInfo);
            var error = await process.StandardError.ReadToEndAsync();
            var output = await process.StandardOutput.ReadToEndAsync();
            var result = output.Trim('\r', '\n', ' ');
            if (string.IsNullOrEmpty(result)) throw new InvalidOperationException(error);
            return result;
        }
    }
}
