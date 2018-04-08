using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Tadmor.Extensions
{
    public static class ExceptionExtensions
    {
        public static string ToShortString(this Exception e)
        {
            var (filePath, line) = new StackTrace(e, true).GetFrames()
                .Select(frame => (filePath: frame.GetFileName(), line: frame.GetFileLineNumber()))
                .FirstOrDefault(i => i.filePath != null);
            var fileInfo = filePath == null ? string.Empty : $" in {Path.GetFileName(filePath)} at line {line}";
            return $"{e.Message} ({e.GetType().Name}{fileInfo})";
        }
    }
}