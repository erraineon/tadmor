using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Tadmor.Extensions
{
    public static class ExceptionExtensions
    {
        public static string ToShortString(this Exception ex)
        {
            var builder = new StringBuilder(ex.Message);
            var (path, line) = new StackTrace(ex, true).GetFrames()
                .Select(frame => (path: frame.GetFileName(), line: frame.GetFileLineNumber()))
                .FirstOrDefault(t => t.path != null);
            if (path != null) builder.Append($" in {Path.GetFileName(path)} at line {line}");
            return builder.ToString();
        }
    }
}