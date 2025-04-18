using DjVuLibreViewer.Core;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using Size = System.Drawing.Size;

namespace DjVuLibreViewer.Drawing
{
    public class DjVuImage : Image
    {
        public DjVuRenderer Renderer { get; set; }
        public int PageNo { get; set; }
        public DjVuPageLinks PageLinks { get; set; }
        private Adorner _adorner = null;

        public DjVuImage() : base()
        {
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            ClipToBounds = true;
            Focusable = false;
            PreviewMouseLeftButtonDown += DjVuImage_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += DjVuImage_PreviewMouseLeftButtonUp;
            PreviewMouseMove += DjVuImage_PreviewMouseMove;
        }

        public void AddAdorner()
        {
            // Create an adorner to this Frame
            if (_adorner == null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(Renderer);
                _adorner = new DjVuImageAdorner(this);
                layer?.Add(_adorner);
            }
        }

        public void RemoveAdorner()
        {
            // Remove adorner
            if (_adorner != null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(Renderer);
                layer?.Remove(_adorner);
                _adorner = null;
            }
        }

        private void DjVuImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Renderer.CursorMode == DjVuCursorMode.TextSelection)
            {
                var viewSize = new Size((int)Width, (int)Height);
                var location = e.GetPosition(this);
                if (e.ClickCount == 1)
                {
                    Renderer.HandleMouseDownForTextSelection(this, PageNo, viewSize, location);
                }
                else if (e.ClickCount == 2)
                {
                    Renderer.HandleMouseDoubleClickForTextSelection(this, PageNo, viewSize, location);
                }
            }
        }

        private void DjVuImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var viewSize = new Size((int)Width, (int)Height);
            var location = e.GetPosition(this);
            Renderer.HandleMouseUpForLinks(this, PageNo, viewSize, location);

            if (Renderer.CursorMode == DjVuCursorMode.TextSelection)
            {
                Renderer.HandleMouseUpForTextSelection(this);
            }
        }

        private void DjVuImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Renderer.CursorMode == DjVuCursorMode.TextSelection)
            {
                var viewSize = new Size((int)Width, (int)Height);
                var location = e.GetPosition(this);
                Renderer.HandleMouseMoveForTextSelection(this, PageNo, viewSize, location);
            }
        }
    }
}
