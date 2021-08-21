using System.IO;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public static class TelegramMarkdownConverter
    {
        public static string ConvertToHtml(string markdown)
        {
            var markdownDocument = Markdown.Parse(markdown);
            var stringWriter = new StringWriter();
            var rr = new HtmlRenderer(stringWriter) { EnableHtmlForBlock = false, };
            rr.ObjectRenderers.ReplaceOrAdd<ParagraphRenderer>(new LineBreakPreservingParagraphRenderer());
            rr.Write(markdownDocument);
            var html = stringWriter.ToString();
            return html;
        }
    }

    public class LineBreakPreservingParagraphRenderer : HtmlObjectRenderer<ParagraphBlock>
    {
        protected override void Write(HtmlRenderer renderer, ParagraphBlock obj)
        {
            if (!renderer.IsFirstInContainer) renderer.WriteLine();
            renderer.WriteLeafInline(obj);
            renderer.EnsureLine();
        }
    }
}