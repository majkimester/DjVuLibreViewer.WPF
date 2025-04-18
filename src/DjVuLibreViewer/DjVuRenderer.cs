using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DjVuLibreViewer.Core;
using DjVuLibreViewer.Drawing;
using DjVuLibreViewer.Enums;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;
using PointF = System.Drawing.PointF;
using System.Windows.Documents;

namespace DjVuLibreViewer
{
    public partial class DjVuRenderer : ScrollPanel
    {
        public DjVuRenderer()
        {
            IsTabStop = true;
            Markers = new DjVuMarkerCollection();
        }

        /// <summary>
        /// Gets a collection with all markers.
        /// </summary>
        public DjVuMarkerCollection Markers { get; }
        private Dictionary<int, List<IDjVuMarker>> _markersByPage;

        public void OpenDjVu(string path, bool isRightToLeft = false)
        {
            UnLoad();
            IsRightToLeft = isRightToLeft;
            Document = DjVuDocument.Load(path);
            OnPagesDisplayModeChanged();
            GotoPage(0, true);
        }

        public void UnLoad()
        {
            PageNo = 0;
            Document?.Dispose();
            Document = null;
            Frames = null;
            Markers.Clear();
            _markersByPage = null;
            Panel.Children.Clear();
            GC.Collect();
        }

        public void ClockwiseRotate()
        {
            // _____
            //      |
            //      |
            //      v
            // Clockwise

            switch (Rotate)
            {
                case DjVuRotation.Rotate0:
                    RotatePage(PageNo, DjVuRotation.Rotate90);
                    break;
                case DjVuRotation.Rotate90:
                    RotatePage(PageNo, DjVuRotation.Rotate180);
                    break;
                case DjVuRotation.Rotate180:
                    RotatePage(PageNo, DjVuRotation.Rotate270);
                    break;
                case DjVuRotation.Rotate270:
                    RotatePage(PageNo, DjVuRotation.Rotate0);
                    break;
            }
        }

        public void Counterclockwise()
        {
            //      ^
            //      |
            //      |
            // _____|
            // Counterclockwise

            switch (Rotate)
            {
                case DjVuRotation.Rotate0:
                    RotatePage(PageNo, DjVuRotation.Rotate270);
                    break;
                case DjVuRotation.Rotate90:
                    RotatePage(PageNo, DjVuRotation.Rotate0);
                    break;
                case DjVuRotation.Rotate180:
                    RotatePage(PageNo, DjVuRotation.Rotate90);
                    break;
                case DjVuRotation.Rotate270:
                    RotatePage(PageNo, DjVuRotation.Rotate180);
                    break;
            }
        }

        /// <summary>
        /// Scroll the DjVu bounds into view.
        /// </summary>
        /// <param name="bounds">The DjVu bounds to scroll into view.</param>
        public void ScrollIntoView(DjVuRectangle bounds)
        {
            Rect? bound = BoundsFromDjVu(bounds);
            if (bound != null)
            {
                ScrollIntoView(bounds.Page, bound.Value);
            }
        }

        /// <summary>
        /// Scroll the client rectangle into view.
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="rectangle">The client rectangle to scroll into view.</param>
        public void ScrollIntoView(int page, Rect rectangle)
        {
            var clientArea = GetScrollClientArea();
            if (ScrollableWidth > 0)
            {
                double horizontalOffset = rectangle.X - clientArea.Width / 2;
                if (PagesDisplayMode == DjVuPagesDisplayMode.BookMode)
                {
                    if ((!IsRightToLeft && page == PageNoLast) || (IsRightToLeft && page == PageNo))
                    {
                        horizontalOffset += Frame1.Width + FrameSpace.Left + FrameSpace.Right;
                    }
                }
                ScrollToHorizontalOffset(horizontalOffset);
            }

            if (ScrollableHeight > 0 && ZoomMode != DjVuZoomMode.FitHeight)
            {
                double verticalOffset = GetPageVerticalOffset(page);
                verticalOffset += rectangle.Y - clientArea.Height / 2;
                ScrollToVerticalOffset(verticalOffset);
            }
        }

        /// <summary>
        /// Converts DjVu bounds to client bounds.
        /// </summary>
        /// <param name="bounds">The DjVu bounds to convert.</param>
        /// <returns>The bounds of the DjVu bounds in client coordinates.</returns>
        public Rect? BoundsFromDjVu(DjVuRectangle bounds)
        {
            return BoundsFromDjVu(bounds, true);
        }

        private Rect? BoundsFromDjVu(DjVuRectangle bounds, bool translateOffset)
        {
            DjVuImage frame;
            if (PagesDisplayMode == DjVuPagesDisplayMode.ContinuousMode)
            {
                frame = Frames[bounds.Page];
            }
            else
            {
                frame = Frame1;
                if (frame?.PageNo != bounds.Page)
                {
                    frame = Frame2;
                }
            }
            if (frame == null) return null;

            var pageBoundsSize = new Size((int)frame.Width, (int)frame.Height);

            var pageSize = Document.PageSizes[bounds.Page];

            var translated = Document.RectangleFromDjVu(
                bounds.Page,
                bounds.Bounds
            );

            var topLeft = TranslatePointFromDjVu(pageBoundsSize, pageSize, new PointF(translated.Left, translated.Top));
            var bottomRight = TranslatePointFromDjVu(pageBoundsSize, pageSize, new PointF(translated.Right, translated.Bottom));

            return new Rect(
                Math.Min(topLeft.X, bottomRight.X),
                Math.Min(topLeft.Y, bottomRight.Y),
                Math.Abs(bottomRight.X - topLeft.X),
                Math.Abs(bottomRight.Y - topLeft.Y)
            );
        }

        private PointF TranslatePointToDjVu(Size size, SizeF pageSize, Point point)
        {
            switch (Rotate)
            {
                case DjVuRotation.Rotate90:
                    point = new Point(size.Height - point.Y, point.X);
                    size = new Size(size.Height, size.Width);
                    break;
                case DjVuRotation.Rotate180:
                    point = new Point(size.Width - point.X, size.Height - point.Y);
                    break;
                case DjVuRotation.Rotate270:
                    point = new Point(point.Y, size.Width - point.X);
                    size = new Size(size.Height, size.Width);
                    break;
            }

            return new PointF(
                ((float)point.X / size.Width) * pageSize.Width,
                ((float)point.Y / size.Height) * pageSize.Height
            );
        }

        private Point TranslatePointFromDjVu(Size size, SizeF pageSize, PointF point)
        {
            switch (Rotate)
            {
                case DjVuRotation.Rotate90:
                    point = new PointF(pageSize.Height - point.Y, point.X);
                    pageSize = new SizeF(pageSize.Height, pageSize.Width);
                    break;
                case DjVuRotation.Rotate180:
                    point = new PointF(pageSize.Width - point.X, pageSize.Height - point.Y);
                    break;
                case DjVuRotation.Rotate270:
                    point = new PointF(point.Y, pageSize.Width - point.X);
                    pageSize = new SizeF(pageSize.Height, pageSize.Width);
                    break;
            }

            return new Point(
                (int)((point.X / pageSize.Width) * size.Width),
                (int)((point.Y / pageSize.Height) * size.Height)
            );
        }

        private Size GetScrollOffset()
        {
            var bounds = GetScrollClientArea();
            return new Size((int)bounds.Width, (int)bounds.Height);
        }

        private Rect GetScrollClientArea()
        {
            return new Rect(0, 0, (int)ViewportWidth, (int)ViewportHeight);
        }

        public void EnsureMarkers()
        {
            if (_markersByPage != null)
                return;

            _markersByPage = new Dictionary<int, List<IDjVuMarker>>();

            foreach (var marker in Markers)
            {
                if (marker.Page < 0 || marker.Page >= PageCount)
                    continue;

                List<IDjVuMarker> pageMarkers;
                _markersByPage.TryGetValue(marker.Page, out pageMarkers);
                if (pageMarkers == null)
                {
                    pageMarkers = new List<IDjVuMarker>();
                    _markersByPage.Add(marker.Page, pageMarkers);
                }
                pageMarkers.Add(marker);
            }
        }

        public void DrawMarkers(DrawingContext graphics, int page)
        {
            List<IDjVuMarker> pageMarkers = null;
            _markersByPage?.TryGetValue(page, out pageMarkers);
            if (pageMarkers == null) return;
            foreach (var marker in pageMarkers)
            {
                marker.Draw(this, graphics);
            }
        }

        public void UpdateAdorner()
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            layer?.Update();
        }

        public void RedrawMarkers()
        {
            _markersByPage = null;
            UpdateAdorner();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(true);
                if (!Dispatcher.HasShutdownStarted)
                {
                    Dispatcher.Invoke(UnLoad);
                }
                GC.SuppressFinalize(this);
                GC.Collect();
            }
        }

        ~DjVuRenderer() => Dispose(true);
    }
}
