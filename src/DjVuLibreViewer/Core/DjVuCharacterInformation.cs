using System.Drawing;

namespace DjVuLibreViewer.Core
{
    public struct DjVuCharacterInformation
    {
        public int Page { get; }
        public int Offset { get; }
        public double FontSize { get; }
        public char Character { get; }
        public RectangleF Bounds { get; }

        public DjVuCharacterInformation(int page, int offset, char character, double fontSize, RectangleF bounds)
        {
            Page = page;
            Offset = offset;
            FontSize = fontSize;
            Bounds = bounds;
            Character = character;
        }
    }
}
