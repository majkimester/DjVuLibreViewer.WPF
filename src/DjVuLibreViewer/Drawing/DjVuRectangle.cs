using System;
using System.Drawing;

namespace DjVuLibreViewer.Drawing
{
    public struct DjVuRectangle : IEquatable<DjVuRectangle>
    {
        public static readonly DjVuRectangle Empty = new DjVuRectangle();

        // _page is offset by 1 so that Empty returns an invalid rectangle.
        private readonly int _page;

        public int Page => _page - 1;

        public RectangleF Bounds { get; }

        public bool IsValid => _page != 0;

        public DjVuRectangle(int page, RectangleF bounds)
        {
            _page = page + 1;
            Bounds = bounds;
        }

        public bool Equals(DjVuRectangle other)
        {
            return
                Page == other.Page &&
                Bounds == other.Bounds;
        }

        public override bool Equals(object obj)
        {
            return
                obj is DjVuRectangle rectangle &&
                Equals(rectangle);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Page * 397) ^ Bounds.GetHashCode();
            }
        }

        public static bool operator ==(DjVuRectangle left, DjVuRectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DjVuRectangle left, DjVuRectangle right)
        {
            return !left.Equals(right);
        }
    }
}
