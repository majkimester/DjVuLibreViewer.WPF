using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using DjVuLibreViewer.Drawing;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace DjVuLibreViewer.Core
{
    public class DjVuMarker : IDjVuMarker
    {
        public int Page { get; }
        public RectangleF Bounds { get; }
        public Color Color { get; }
        public Color BorderColor { get; }
        public float BorderWidth { get; }

        public DjVuMarker(int page, RectangleF bounds, Color color)
            : this(page, bounds, color, Colors.Transparent, 0)
        {
        }

        public DjVuMarker(int page, RectangleF bounds, Color color, Color borderColor, float borderWidth)
        {
            Page = page;
            Bounds = bounds;
            Color = color;
            BorderColor = borderColor;
            BorderWidth = borderWidth;
        }

        public void Draw(DjVuRenderer renderer, DrawingContext graphics)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));

            Rect? bounds = renderer.BoundsFromDjVu(new DjVuRectangle(Page, Bounds));
            if (bounds == null) return;

            var brush = new SolidColorBrush(Color) { Opacity = .5 };
            graphics.DrawRectangle(brush, null, bounds.Value);

            if (BorderWidth > 0)
            {
                var pen = new Pen(new SolidColorBrush(BorderColor) { Opacity = .8 }, BorderWidth);
                graphics.DrawRectangle(null, pen, bounds.Value);
            }
        }
    }
}
