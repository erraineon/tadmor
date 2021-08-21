using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tadmor.Core.ChatClients.Telegram.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class TelegramMarkdownParserTests
    {
        private readonly string _input = @"line 1
line 2 after one line break

line 3 after two line breaks
line 4 after two line breaks


**line 5 after three line *breaks***";

        [TestMethod]
        public void GeneralTest_RespectsMarkdown_AndMultipleLineBreaks()
        {
            var html = TelegramMarkdownConverter.ConvertToHtml(_input);
            Trace.WriteLine(html);
        }
    }
}