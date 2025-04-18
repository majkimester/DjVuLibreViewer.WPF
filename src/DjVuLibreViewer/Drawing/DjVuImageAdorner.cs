using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DjVuLibreViewer.Drawing
{
    // Adorners must subclass the abstract base class Adorner.
    public class DjVuImageAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public DjVuImageAdorner(UIElement adornedElement) : base(adornedElement)
        {
            IsClipEnabled = true;
            IsHitTestVisible = false;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            DjVuImage image = AdornedElement as DjVuImage;
            if (image.Source != null)
            {
                //Debug.WriteLine("DjVuImageAdorner.OnRender[" + image.PageNo + "]");
                image.Renderer.EnsureMarkers();
                image.Renderer.DrawMarkers(drawingContext, image.PageNo);

                image.Renderer.DrawTextSelection(drawingContext, image.PageNo, image.Renderer.TextSelectionState);

                // Draw simple border
                var pen = new Pen(new SolidColorBrush(Colors.Black), 1);
                drawingContext.DrawRectangle(null, pen, new Rect(0, 0, image.Width, image.Height));
            }
        }
    }
}
