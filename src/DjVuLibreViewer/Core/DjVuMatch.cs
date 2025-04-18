using DjVuLibreViewer.Drawing;

namespace DjVuLibreViewer.Core
{
    public class DjVuMatch
    {
        public string Text { get; }
        public DjVuTextSpan TextSpan { get; }
        public int Page { get; }

        public DjVuMatch(string text, DjVuTextSpan textSpan, int page)
        {
            Text = text;
            TextSpan = textSpan;
            Page = page;
        }
    }
}
