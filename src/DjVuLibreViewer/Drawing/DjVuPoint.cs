using System;
using System.Drawing;

namespace DjVuLibreViewer.Drawing
{
    public struct DjVuPoint : IEquatable<DjVuPoint>
    {
        public static readonly DjVuPoint Empty = new DjVuPoint();

        // _page is offset by 1 so that Empty returns an invalid point.
        private readonly int _page;

        public int Page
        {
            get { return _page - 1; }
        }

        public PointF Location { get; }

        public bool IsValid
        {
            get { return _page != 0; }
        }

        public DjVuPoint(int page, PointF location)
        {
            _page = page + 1;
            Location = location;
        }

        public bool Equals(DjVuPoint other)
        {
            return
                Page == other.Page &&
                Location == other.Location;
        }

        public override bool Equals(object obj)
        {
            return
                obj is DjVuPoint &&
                Equals((DjVuPoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Page * 397) ^ Location.GetHashCode();
            }
        }

        public static bool operator ==(DjVuPoint left, DjVuPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DjVuPoint left, DjVuPoint right)
        {
            return !left.Equals(right);
        }
    }
}
