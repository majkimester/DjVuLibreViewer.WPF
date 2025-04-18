using System;

namespace DjVuLibreViewer.Drawing
{
    public struct DjVuTextSpan : IEquatable<DjVuTextSpan>
    {
        public int Page { get; }
        public int Offset { get; }
        public int Length { get; }

        public DjVuTextSpan(int page, int offset, int length)
        {
            Page = page;
            Offset = offset;
            Length = length;
        }

        public bool Equals(DjVuTextSpan other)
        {
            return
                Page == other.Page &&
                Offset == other.Offset &&
                Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            return
                obj is DjVuTextSpan span &&
                Equals(span);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Page;
                hashCode = (hashCode * 397) ^ Offset;
                hashCode = (hashCode * 397) ^ Length;
                return hashCode;
            }
        }

        public static bool operator ==(DjVuTextSpan left, DjVuTextSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DjVuTextSpan left, DjVuTextSpan right)
        {
            return !left.Equals(right);
        }
    }
}
