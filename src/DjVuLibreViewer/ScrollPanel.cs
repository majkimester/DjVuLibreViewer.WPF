﻿using DjVuLibreViewer.Core;
using DjVuLibreViewer.Drawing;
using DjVuLibreViewer.Enums;
using DjVuLibreViewer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Size = System.Drawing.Size;

namespace DjVuLibreViewer
{
    // ScrollPanel.Properties
    public partial class ScrollPanel : ScrollViewer, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public ScrollPanel()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Panel = new StackPanel()
            {
                SnapsToDevicePixels = true,
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            VirtualizingPanel.SetIsVirtualizing(Panel, true);
            VirtualizingPanel.SetVirtualizationMode(Panel, VirtualizationMode.Recycling);
            Content = Panel;
            Dpi = (int)VisualTreeHelper.GetDpi(this).PixelsPerInchX;
        }

        public event EventHandler<int> PageChanged;
        public event EventHandler MouseClick;
        protected bool IsDisposed;
        protected const int ScrollChangeLine = 16;
        protected Process CurrentProcess { get; } = Process.GetCurrentProcess();
        protected StackPanel Panel { get; set; }
        protected Thickness FrameSpace { get; set; } = new Thickness(5);
        protected DjVuImage Frame1 => Frames?.FirstOrDefault();
        protected DjVuImage Frame2 => Frames?.Length > 1 ? Frames[1] : null;
        protected DjVuImage[] Frames { get; set; }
        protected int ScrollWidth { get; set; } = 50;
        protected int MouseWheelDelta { get; set; }
        protected long MouseWheelUpdateTime { get; set; }

        /// <summary>
        /// Current document.
        /// </summary>
        public DjVuDocument Document {
            get => _document;
            set {
                SetProperty(ref _document, value);
                OnPropertyChanged(nameof(PageCount));
                OnPropertyChanged(nameof(IsDocumentLoaded));
            }
        }
        private DjVuDocument _document;

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int PageCount => Document?.PageCount ?? 0;
        
        /// <summary>
        /// Number of the current page.
        /// </summary>
        public int PageNo { get => _pageNo; protected set => SetProperty(ref _pageNo, value); }
        private int _pageNo;

        /// <summary>
        /// Number of the last visible page.
        /// </summary>
        public int PageNoLast { get => _pageNoLast; protected set => SetProperty(ref _pageNoLast, value); }
        private int _pageNoLast;

        /// <summary>
        /// Page sizes.
        /// </summary>
        public IList<SizeF> PageSizes => Document?.PageSizes;

        /// <summary>
        /// Size of the current page.
        /// </summary>
        public Size CurrentPageSize { get => _currentPageSize; set => SetProperty(ref _currentPageSize, value); }
        private Size _currentPageSize;

        /// <summary>
        /// Pixel density.
        /// </summary>
        public int Dpi { get => _dpi; set => SetProperty(ref _dpi, value); }
        private int _dpi;

        /// <summary>
        /// Page rotation.
        /// </summary>
        public DjVuRotation Rotate { get; set; } = DjVuRotation.Rotate0;

        /// <summary>
        /// Page display mode (SinglePageMode, BookMode or ContinuousMode)
        /// </summary>
        public DjVuPagesDisplayMode PagesDisplayMode {
            get => _DjVuViewerPagesDisplayMode;
            set {
                if (SetProperty(ref _DjVuViewerPagesDisplayMode, value))
                {
                    OnPagesDisplayModeChanged();
                }
            }
        }
        private DjVuPagesDisplayMode _DjVuViewerPagesDisplayMode = DjVuPagesDisplayMode.ContinuousMode;

        /// <summary>
        /// Mouse wheel mode (PanAndZoom, Pan or Zoom)
        /// </summary>
        public MouseWheelMode MouseWheelMode { get; set; } = MouseWheelMode.PanAndZoom;

        /// <summary>
        /// Is right to left layout.
        /// </summary>
        public bool IsRightToLeft
        {
            get => Panel.FlowDirection == FlowDirection.RightToLeft;
            set => Panel.FlowDirection = value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        /// <summary>
        /// Document loaded.
        /// </summary>
        public bool IsDocumentLoaded => Document != null && ActualWidth > 0 && ActualHeight > 0;

        /// <summary>
        /// Bookmarks.
        /// </summary>
        public DjVuBookmarkCollection Bookmarks => Document?.Bookmarks;

        /// <summary>
        /// Scroll to selected page in continous mode
        /// </summary>
        /// <param name="page"></param>
        protected void ScrollToPage(int page)
        {
            if (Frames == null) return;

            if (PagesDisplayMode == DjVuPagesDisplayMode.ContinuousMode)
            {
                // scroll to current page
                double verticalOffset = GetPageVerticalOffset(page);
                if (verticalOffset == VerticalOffset) verticalOffset++;
                ScrollToVerticalOffset(verticalOffset);
            }
        }

        protected double GetPageVerticalOffset(int page)
        {
            double verticalOffset = 0.0;
            if (PagesDisplayMode == DjVuPagesDisplayMode.ContinuousMode)
            {
                for (int idx = 0; idx < page; idx++)
                {
                    verticalOffset += Frames[idx].Height + FrameSpace.Top + FrameSpace.Bottom;
                }
            }
            return verticalOffset;
        }

        protected void OnPageNoChanged()
        {
            PageChanged?.Invoke(this, PageNo);
        }

        protected void OnDpiChanged()
        {
            GotoPage(PageNo);
        }

        protected void OnPagesDisplayModeChanged()
        {
            if (IsDocumentLoaded)
            {
                Panel.Children.Clear();
                Frames = null;

                if (PagesDisplayMode == DjVuPagesDisplayMode.SinglePageMode)
                {
                    Frames = new DjVuImage[1];
                    Panel.Orientation = Orientation.Horizontal;
                }
                else if (PagesDisplayMode == DjVuPagesDisplayMode.BookMode)
                {
                    Frames = new DjVuImage[2];
                    Panel.Orientation = Orientation.Horizontal;
                }
                else if (PagesDisplayMode == DjVuPagesDisplayMode.ContinuousMode)
                {
                    // frames created at scrolling
                    Frames = new DjVuImage[Document.PageCount];
                    Panel.Orientation = Orientation.Vertical;
                }

                for (var i = 0; i < Frames.Length; i++)
                {
                    Frames[i] = Frames[i] ?? new DjVuImage { Margin = FrameSpace, Renderer=this as DjVuRenderer, PageNo = i };

                    var pageSize = CalculatePageSize(i);
                    Frames[i].Width = pageSize.Width;
                    Frames[i].Height = pageSize.Height;

                    Panel.Children.Add(Frames[i]);
                }

                GotoPage(PageNo);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            MouseClick?.Invoke(this, EventArgs.Empty);
        }

        protected void OnFlagsChanged()
        {
            GotoPage(PageNo);
        }

        protected void RenderPage(DjVuImage frame, int page, int width, int height)
        {
            if (frame == null || width == 0 || height == 0) return;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var bitmapSource = Document.Render(page, width, height, Rotate);
            var partTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Stop();
            Debug.WriteLine("RenderPage[" + page + "]: " + (stopwatch.ElapsedMilliseconds) + " mS, (" + (stopwatch.ElapsedMilliseconds - partTime) +" mS) size=" + width + " x " + height);

            Dispatcher.Invoke(() =>
            {
                frame.Width = width;
                frame.Height = height;
                frame.Source = bitmapSource;
                frame.PageNo = page;
                frame.PageLinks = GetPageLinks(page, new Size(width, height));
            });
        }

        protected Size CalculatePageSize(int? page = null)
        {
            page = page ?? PageNo;
            var isReverse = (Rotate == DjVuRotation.Rotate90 || Rotate == DjVuRotation.Rotate270);
            var containerWidth = ActualWidth - Padding.Left - Padding.Right - FrameSpace.Left - FrameSpace.Right; // ViewportWidth
            var containerHeight = ActualHeight - Padding.Top - Padding.Bottom - FrameSpace.Top - FrameSpace.Bottom; // ViewportHeight

            if (IsDocumentLoaded && containerWidth > 0 && containerHeight > 0 && containerHeight < 10000)
            {
                var currentPageSize = Document.GetPageSize(page.Value);
                if (isReverse)
                    currentPageSize = new SizeF(currentPageSize.Height, currentPageSize.Width);

                if (ZoomMode == DjVuZoomMode.FitHeight)
                {
                    _zoom = containerHeight / currentPageSize.Height;
                    double width = _zoom * currentPageSize.Width;
                    if (PagesDisplayMode == DjVuPagesDisplayMode.BookMode) width *= 2;
                    if (width > containerWidth)
                    {
                        _zoom = (containerHeight - ScrollWidth) / currentPageSize.Height;
                    }
                }
                else if (ZoomMode == DjVuZoomMode.FitWidth)
                {
                    _zoom = (containerWidth - ScrollWidth) / currentPageSize.Width;
                    if (PagesDisplayMode == DjVuPagesDisplayMode.BookMode)
                    {
                        _zoom /= 2;
                    }
                }

                return new Size((int)(currentPageSize.Width * Zoom), (int)(currentPageSize.Height * Zoom));
            }

            return new Size();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            OnPagesDisplayModeChanged();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            SetMouseWheelDelta(e.Delta);
            
            if (IsDocumentLoaded)
            {
                if (MouseWheelMode == MouseWheelMode.Zoom)
                {
                    e.Handled = true;
                    if (e.Delta > 0)
                        ZoomIn();
                    else
                        ZoomOut();
                }
                else if (PagesDisplayMode != DjVuPagesDisplayMode.ContinuousMode)
                {
                    var pageStep = PagesDisplayMode == DjVuPagesDisplayMode.BookMode ? 2 : 1;

                    if (ViewportHeight > Frame1.ActualHeight)
                    {
                        if (e.Delta > 0) // prev page
                            PreviousPage();
                        else
                            NextPage();
                    }
                    else if (e.Delta < 0 && VerticalOffset >= ScrollableHeight && PageNo < PageCount - pageStep)
                    {
                        NextPage();
                        ScrollToVerticalOffset(0);
                    }
                    else if (e.Delta > 0 && VerticalOffset <= 0 && PageNo > 0)
                    {
                        PreviousPage();
                        ScrollToVerticalOffset(ScrollableHeight);
                    }
                }
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            bool handled = true;

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                MouseWheelMode = MouseWheelMode.Zoom;

            switch (e.Key)
            {
                case Key.Up:
                    PerformScroll(ScrollAction.LineUp, Orientation.Vertical);
                    break;

                case Key.Down:
                    PerformScroll(ScrollAction.LineDown, Orientation.Vertical);
                    break;

                case Key.Left:
                    PerformScroll(ScrollAction.LineUp, Orientation.Horizontal);
                    break;

                case Key.Right:
                    PerformScroll(ScrollAction.LineDown, Orientation.Horizontal);
                    break;

                case Key.PageUp:
                    PerformScroll(ScrollAction.PageUp, Orientation.Vertical);
                    break;

                case Key.PageDown:
                    PerformScroll(ScrollAction.PageDown, Orientation.Vertical);
                    break;

                case Key.Home:
                    PerformScroll(ScrollAction.Home, Orientation.Vertical);
                    break;

                case Key.End:
                    PerformScroll(ScrollAction.End, Orientation.Vertical);
                    break;

                case Key.Add:
                case Key.OemPlus:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        ZoomIn();
                    break;

                case Key.Subtract:
                case Key.OemMinus:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        ZoomOut();
                    break;
                default:
                    handled = false;
                    break;
            }
            e.Handled = handled;
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control ||
                e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                MouseWheelMode = MouseWheelMode.Pan;
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);
            if (IsDocumentLoaded &&
                PagesDisplayMode == DjVuPagesDisplayMode.ContinuousMode &&
                Frames?.Length > 0 &&
                e.VerticalChange != 0)
            {
                // Render frames for continous mode
                double startOffset = e.VerticalOffset;
                double endOffset = startOffset + e.ViewportHeight;
                bool isPageNoSet = false;

                double position = 0;
                int pageNoLast = 0;
                for (int page = 0; page < Frames.Length; page++)
                {
                    var pageHeightWithFrame = Frames[page].Height + FrameSpace.Top + FrameSpace.Bottom;
                    var pageStartPos = position;
                    var pageEndPos = position + pageHeightWithFrame;
                    var frame = Frames[page];

                    if ((pageStartPos >= startOffset && pageStartPos < endOffset)      // Page top is inside the viewport range
                        || (pageEndPos > startOffset && pageEndPos < endOffset)        // Page bottom is inside the viewport range
                        || (pageStartPos < startOffset && pageEndPos >= endOffset))    // Page top is above the viewport and page bottom is below the viewport
                    {
                        // Page visible, render if not exist
                        if (frame.Source == null) 
                        {
                            RenderPage(frame, page, (int)frame.Width, (int)frame.Height);
                            frame.AddAdorner();
                            pageNoLast = page;
                        }
                        if (!isPageNoSet)
                        {
                            PageNo = page;
                            isPageNoSet = true;
                        }
                    }
                    else
                    {
                        // Page not visible, release it
                        if (frame.Source != null)
                        {
                            // Debug.WriteLine("Release Page[" + page + "]");
                            frame.Source = null;
                            frame.RemoveAdorner();
                        }
                    }
                    position += pageHeightWithFrame;
                    PageNoLast = pageNoLast;
                }
            }
        }

        public int GetPageNumberFromPosition(System.Windows.Point mousePos)
        {
            if (IsDocumentLoaded &&
                PagesDisplayMode == DjVuPagesDisplayMode.ContinuousMode &&
                Frames?.Length > 0)
            {
                // Determine frame
                double position = 0;
                for (int page = 0; page < Frames.Length; page++)
                {
                    var pageHeightWithFrame = Frames[page].Height + FrameSpace.Top + FrameSpace.Bottom;
                    var pageWidthWithFrame = Frames[page].Width + FrameSpace.Left + FrameSpace.Right;
                    var pageStartPos = position;
                    var pageEndPos = position + pageHeightWithFrame;
                    var hCenter = ViewportWidth / 2;
                    var pageLeft = hCenter - pageWidthWithFrame / 2;
                    var pageRight = hCenter + pageWidthWithFrame / 2;

                    if (mousePos.X >= pageLeft && mousePos.X < pageRight && mousePos.Y >= pageStartPos && mousePos.Y < pageEndPos)
                    {
                        return page;
                    }
                    position += pageHeightWithFrame;
                }
            }
            return -1;
        }

        public void PerformScroll(ScrollAction action, Orientation scrollBar)
        {
            if (scrollBar == Orientation.Vertical)
            {
                switch (action)
                {
                    case ScrollAction.LineUp:
                        if (VerticalOffset > ScrollChangeLine)
                            ScrollToVerticalOffset(VerticalOffset - ScrollChangeLine);
                        break;

                    case ScrollAction.LineDown:
                        if (VerticalOffset < ScrollableHeight - ScrollChangeLine)
                            ScrollToVerticalOffset(VerticalOffset + ScrollChangeLine);
                        break;

                    case ScrollAction.PageUp:
                        if (ZoomMode == DjVuZoomMode.FitHeight || ScrollableHeight == 0)
                        {
                            PreviousPage();
                        }
                        else if (PagesDisplayMode != DjVuPagesDisplayMode.ContinuousMode && VerticalOffset == 0)
                        {
                            // In single page / book mode we are on the top of the page, load the bottom of the previous page
                            PreviousPage();
                            ScrollToVerticalOffset(ScrollableHeight);
                        }
                        else
                        {
                            var offset = ActualHeight - Margin.Top - Margin.Bottom - FrameSpace.Top - FrameSpace.Bottom;
                            if (VerticalOffset > offset)
                                ScrollToVerticalOffset(VerticalOffset - offset);
                            else
                                ScrollToVerticalOffset(0);
                        }
                        break;

                    case ScrollAction.PageDown:
                        if (ZoomMode == DjVuZoomMode.FitHeight || ScrollableHeight == 0)
                        {
                            NextPage();
                        }
                        else if (PagesDisplayMode != DjVuPagesDisplayMode.ContinuousMode && VerticalOffset == ScrollableHeight)
                        {
                            // In single page / book mode we are on the bottom of the page, load the top of the next page
                            NextPage();
                            ScrollToVerticalOffset(0);
                        }
                        else
                        {
                            var offset = ActualHeight - Margin.Top - Margin.Bottom - FrameSpace.Top - FrameSpace.Bottom;
                            if (VerticalOffset < ScrollableHeight - offset)
                                ScrollToVerticalOffset(VerticalOffset + offset);
                            else
                                ScrollToVerticalOffset(ScrollableHeight);
                        }
                        break;

                    case ScrollAction.Home:
                        ScrollToHome();
                        break;

                    case ScrollAction.End:
                        ScrollToEnd();
                        break;
                }
            }
            else // Horizontal
            {
                switch (action)
                {
                    case ScrollAction.LineUp:
                        if (HorizontalOffset > ScrollChangeLine)
                            ScrollToHorizontalOffset(HorizontalOffset - ScrollChangeLine);
                        break;

                    case ScrollAction.LineDown:
                        if (HorizontalOffset < ScrollableHeight - ScrollChangeLine)
                            ScrollToHorizontalOffset(HorizontalOffset + ScrollChangeLine);
                        break;

                    case ScrollAction.PageUp:
                        var offset = ActualWidth - Margin.Left - Margin.Right - FrameSpace.Left - FrameSpace.Right;
                        if (HorizontalOffset > offset)
                            ScrollToHorizontalOffset(HorizontalOffset - offset);
                        else
                            ScrollToHorizontalOffset(0);
                        break;

                    case ScrollAction.PageDown:
                        offset = ActualWidth - Margin.Left - Margin.Right - FrameSpace.Left - FrameSpace.Right;
                        if (HorizontalOffset < ScrollableWidth - offset)
                            ScrollToHorizontalOffset(HorizontalOffset + offset);
                        else
                            ScrollToHorizontalOffset(ScrollableWidth);
                        break;

                    case ScrollAction.Home:
                        ScrollToHome();
                        break;

                    case ScrollAction.End:
                        ScrollToEnd();
                        break;
                }
            }
        }

        protected void SetMouseWheelDelta(int delta)
        {
            MouseWheelUpdateTime = Environment.TickCount;
            MouseWheelDelta = delta;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(!IsDisposed);
        }
    }
}
