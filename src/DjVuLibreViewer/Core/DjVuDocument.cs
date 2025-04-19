using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Media.Imaging;
using DjVuLibreViewer.Drawing;
using DjVuLibreViewer.Enums;
using WriteableBitmap = System.Windows.Media.Imaging.WriteableBitmap;

namespace DjVuLibreViewer.Core
{
    /// <summary>
    /// Provides functionality to render a DjVu document.
    /// </summary>
    public class DjVuDocument : IDjVuDocument
    {
        private bool _disposed;
        private DjVuFile _file;
        private readonly List<SizeF> _pageSizes;

        /// <summary>
        /// Initializes a new instance of the DjVuDocument class with the provided path.
        /// </summary>
        /// <param name="path">Path to the DjVu document.</param>
        public static DjVuDocument Load(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return new DjVuDocument(path);
        }

        /// <summary>
        /// Number of pages in the DjVu document.
        /// </summary>
        public int PageCount
        {
            get { return PageSizes.Count; }
        }

        /// <summary>
        /// Bookmarks stored in this DjVuFile
        /// </summary>
        public DjVuBookmarkCollection Bookmarks
        {
            get { return _file.Bookmarks; }
        }

        /// <summary>
        /// Size of each page in the DjVu document.
        /// </summary>
        public IList<SizeF> PageSizes { get; private set; }

        private DjVuDocument(string path)
        {
            _file = new DjVuFile(path);

            _pageSizes = _file.GetDjVuDocInfo();
            if (_pageSizes == null)
                throw new Win32Exception();

            PageSizes = new ReadOnlyCollection<SizeF>(_pageSizes);
        }

        /// <summary>
        /// Renders a page of the DjVu document to the provided graphics instance.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="graphics">Graphics instance to render the page on.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="bounds">Bounds to render the page in.</param>
        public void Render(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            float graphicsDpiX = graphics.DpiX;
            float graphicsDpiY = graphics.DpiY;

            var dc = graphics.GetHdc();

            try
            {
                if ((int)graphicsDpiX != (int)dpiX || (int)graphicsDpiY != (int)dpiY)
                {
                    var transform = new NativeMethods.XFORM
                    {
                        eM11 = graphicsDpiX / dpiX,
                        eM22 = graphicsDpiY / dpiY
                    };

                    NativeMethods.SetGraphicsMode(dc, NativeMethods.GmAdvanced);
                    NativeMethods.ModifyWorldTransform(dc, ref transform, NativeMethods.MwtLeftMultiply);
                }

                var point = new NativeMethods.POINT();
                NativeMethods.SetViewportOrgEx(dc, bounds.X, bounds.Y, out point);

                bool success = _file.RenderDjVuPageToDC(
                    page,
                    dc,
                    (int)dpiX, (int)dpiY,
                    0, 0, bounds.Width, bounds.Height
                );

                NativeMethods.SetViewportOrgEx(dc, point.X, point.Y, out point);

                if (!success)
                    throw new Win32Exception();
            }
            finally
            {
                graphics.ReleaseHdc(dc);
            }
        }

        /// <summary>
        /// Renders a page of the DjVu document to an image.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <returns>The rendered image.</returns>
        public Image Render(int page, float dpiX, float dpiY)
        {
            var size = PageSizes[page];

            return Render(page, (int)size.Width, (int)size.Height, dpiX, dpiY);
        }

        /// <summary>
        /// Renders a page of the DjVu document to an image.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="width">Width of the rendered image.</param>
        /// <param name="height">Height of the rendered image.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <returns>The rendered image.</returns>
        public Image Render(int page, int width, int height, float dpiX, float dpiY)
        {
            return Render(page, width, height, dpiX, dpiY, 0);
        }

        /// <summary>
        /// Renders a page of the DjVu document to an image.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="width">Width of the rendered image.</param>
        /// <param name="height">Height of the rendered image.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="rotate">Rotation.</param>
        /// <param name="flags">Flags used to influence the rendering.</param>
        /// <param name="drawFormFields">Draw form fields.</param>
        /// <returns>The rendered image.</returns>
        public Image Render(int page, int width, int height, float dpiX, float dpiY, DjVuRotation rotate)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            return null;


            /*
             
                         var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bitmap.SetResolution(dpiX, dpiY);

            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            try
            {
               return _file.RenderDjVuPage(page);

            }
            finally
            {
                bitmap.UnlockBits(data);
            }

             */


            /* TODO
            try
            {
                var handle = NativeMethods.FPDFBitmap_CreateEx(width, height, 4, data.Scan0, width * 4);

                try
                {
                    uint background = (flags & DjVuRenderFlags.Transparent) == 0 ? 0xFFFFFFFF : 0x00FFFFFF;

                    NativeMethods.FPDFBitmap_FillRect(handle, 0, 0, width, height, background);

                    bool success = _file.RenderDjVuPageToBitmap(
                        page,
                        handle,
                        (int)dpiX, (int)dpiY,
                        0, 0, width, height,
                        (int)rotate,
                        FlagsToFPDFFlags(flags),
                        drawFormFields
                    );

                    if (!success)
                        throw new Win32Exception();
                }
                finally
                {
                    NativeMethods.FPDFBitmap_Destroy(handle);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
            */
        }

        /// <summary>
        /// Renders a page of the DjVu document to an image source.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="width">Width of the rendered image.</param>
        /// <param name="height">Height of the rendered image.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="rotate">Rotation.</param>
        /// <param name="flags">Flags used to influence the rendering.</param>
        /// <param name="drawFormFields">Draw form fields.</param>
        /// <returns>The rendered image.</returns>
        public WriteableBitmap Render2(int page, int width, int height, float dpiX, float dpiY, DjVuRotation rotate)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            /*
            if ((flags & DjVuRenderFlags.CorrectFromDpi) != 0)
            {
                width = width * (int)dpiX / 72;
                height = height * (int)dpiY / 72;
            }
            */

            var writeableBitmap = new WriteableBitmap(width, height, dpiX, dpiY, System.Windows.Media.PixelFormats.Bgra32, null);

            /*
            try
            {
                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                IntPtr buffer = writeableBitmap.BackBuffer;

                var handle = NativeMethods.FPDFBitmap_CreateEx(width, height, 4, buffer, width * 4);

                try
                {
                    uint background = (flags & DjVuRenderFlags.Transparent) == 0 ? 0xFFFFFFFF : 0x00FFFFFF;

                    NativeMethods.FPDFBitmap_FillRect(handle, 0, 0, width, height, background);

                    bool success = _file.RenderDjVuPageToBitmap(
                        page,
                        handle,
                        (int)dpiX, (int)dpiY,
                        0, 0, width, height,
                        (int)rotate,
                        FlagsToFPDFFlags(flags),
                        drawFormFields
                    );

                    if (!success)
                        throw new Win32Exception();
                }
                finally
                {
                    NativeMethods.FPDFBitmap_Destroy(handle);
                }

                // Specify the area of the bitmap that changed.
                writeableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
                writeableBitmap.Freeze();
            }
            */

            return writeableBitmap;
        }

        /// <summary>
        /// Renders a page of the DjVu document to an image.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="width">Width of the rendered image.</param>
        /// <param name="height">Height of the rendered image.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="rotate">Rotation.</param>
        /// <returns>The rendered image.</returns>
        public BitmapSource Render3(int page, int width, int height, float dpiX, float dpiY, DjVuRotation rotate)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            return _file.RenderDjVuPage(page);

            /*
            if ((flags & DjVuRenderFlags.CorrectFromDpi) != 0)
            {
                width = width * (int)dpiX / 72;
                height = height * (int)dpiY / 72;
            }
            */

            // Create byte array to hold image data
            //byte[] imageData = new byte[width * height * 4]; // Assuming 32bpp ARGB format
            // GCHandle pinnedArray = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            //IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            /*
            try
            {
                var handle = NativeMethods.FPDFBitmap_CreateEx(width, height, 4, pointer, width * 4);
                try
                {
                    uint background = (flags & DjVuRenderFlags.Transparent) == 0 ? 0xFFFFFFFF : 0x00FFFFFF;

                    NativeMethods.FPDFBitmap_FillRect(handle, 0, 0, width, height, background);

                    bool success = _file.RenderDjVuPageToBitmap(
                        page,
                        handle,
                        (int)dpiX, (int)dpiY,
                        0, 0, width, height,
                        (int)rotate,
                        FlagsToFPDFFlags(flags),
                        drawFormFields
                    );

                    if (!success)
                        throw new Win32Exception();
                }
                finally
                {
                    NativeMethods.FPDFBitmap_Destroy(handle);
                }

                var bitmapSource = BitmapSource.Create(
                    width,
                    height,
                    dpiX,
                    dpiY,
                    PixelFormats.Bgra32,
                    null,
                    imageData,
                    width * 4);
                bitmapSource.Freeze();
                return bitmapSource;
            }
            finally
            {
                pinnedArray.Free();
            }
            */
            //return null;
        }

        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <returns>All matches.</returns>
        public DjVuMatches Search(string text, bool matchCase, bool wholeWord)
        {
            return Search(text, matchCase, wholeWord, 0, PageCount - 1);
        }

        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <param name="page">The page to search on.</param>
        /// <returns>All matches.</returns>
        public DjVuMatches Search(string text, bool matchCase, bool wholeWord, int page)
        {
            return Search(text, matchCase, wholeWord, page, page);
        }

        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <param name="startPage">The page to start searching.</param>
        /// <param name="endPage">The page to end searching.</param>
        /// <returns>All matches.</returns>
        public DjVuMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage)
        {
            return _file.Search(text, matchCase, wholeWord, startPage, endPage);
        }

        /// <summary>
        /// Get all text on the page.
        /// </summary>
        /// <param name="page">The page to get the text for.</param>
        /// <returns>The text on the page.</returns>
        public string GetDjVuText(int page)
        {
            return _file.GetDjVuText(page);
        }

        /// <summary>
        /// Get all text matching the text span.
        /// </summary>
        /// <param name="textSpan">The span to get the text for.</param>
        /// <returns>The text matching the span.</returns>
        public string GetDjVuText(DjVuTextSpan textSpan)
        {
            return _file.GetDjVuText(textSpan);
        }

        /// <summary>
        /// Get all bounding rectangles for the text span.
        /// </summary>
        /// <description>
        /// The algorithm used to get the bounding rectangles tries to join
        /// adjacent character bounds into larger rectangles.
        /// </description>
        /// <param name="textSpan">The span to get the bounding rectangles for.</param>
        /// <returns>The bounding rectangles.</returns>
        public IList<DjVuRectangle> GetTextBounds(DjVuTextSpan textSpan)
        {
            return _file.GetTextBounds(textSpan);
        }

        /// <summary>
        /// Convert a point from device coordinates to page coordinates.
        /// </summary>
        /// <param name="page">The page number where the point is from.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns>The converted point.</returns>
        public PointF PointToDjVu(int page, Point point)
        {
            return _file.PointToDjVu(page, point);
        }

        /// <summary>
        /// Convert a point from page coordinates to device coordinates.
        /// </summary>
        /// <param name="page">The page number where the point is from.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns>The converted point.</returns>
        public Point PointFromDjVu(int page, PointF point)
        {
            return _file.PointFromDjVu(page, point);
        }

        /// <summary>
        /// Convert a rectangle from device coordinates to page coordinates.
        /// </summary>
        /// <param name="page">The page where the rectangle is from.</param>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>The converted rectangle.</returns>
        public RectangleF RectangleToDjVu(int page, Rectangle rect)
        {
            return _file.RectangleToDjVu(page, rect);
        }

        /// <summary>
        /// Convert a rectangle from page coordinates to device coordinates.
        /// </summary>
        /// <param name="page">The page where the rectangle is from.</param>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>The converted rectangle.</returns>
        public Rectangle RectangleFromDjVu(int page, RectangleF rect)
        {
            return _file.RectangleFromDjVu(page, rect);
        }

        /// <summary>
        /// Get the character index at or nearby a specific position. 
        /// </summary>
        /// <param name="page">The page to get the character index from</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="xTolerance">An x-axis tolerance value for character hit detection, in point unit.</param>
        /// <param name="yTolerance">A y-axis tolerance value for character hit detection, in point unit.</param>
        /// <returns>The zero-based index of the character at, or nearby the point specified by parameter x and y. If there is no character at or nearby the point, it will return -1.</returns>
        public int GetCharacterIndexAtPosition(DjVuPoint location, double xTolerance, double yTolerance)
        {
            return _file.GetCharIndexAtPos(location, xTolerance, yTolerance);
        }

        /// <summary>
        /// Get the full word at or nearby a specific position
        /// </summary>
        /// <param name="location">The location to inspect</param>
        /// <param name="xTolerance">An x-axis tolerance value for character hit detection, in point unit.</param>
        /// <param name="yTolerance">A y-axis tolerance value for character hit detection, in point unit.</param>
        /// <param name="span">The location of the found word, if any</param>
        /// <returns>A value indicating whether a word was found at the specified location</returns>
        public bool GetWordAtPosition(DjVuPoint location, double xTolerance, double yTolerance, out DjVuTextSpan span)
        {
            return _file.GetWordAtPosition(location, xTolerance, yTolerance, out span);
        }

        /// <summary>
        /// Get number of characters in a page.
        /// </summary>
        /// <param name="page">The page to get the character count from</param>
        /// <returns>Number of characters in the page. Generated characters, like additional space characters, new line characters, are also counted.</returns>
        public int CountCharacters(int page)
        {
            return _file.CountChars(page);
        }

        /// <summary>
        /// Gets the rectangular areas occupied by a segment of text
        /// </summary>
        /// <param name="page">The page to get the rectangles from</param>
        /// <returns>The rectangular areas occupied by a segment of text</returns>
        public List<DjVuRectangle> GetTextRectangles(int page, int startIndex, int count)
        {
            return _file.GetTextRectangles(page, startIndex, count);
        }

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the DjVu document.
        /// </summary>
        /// <returns></returns>
        public PrintDocument CreatePrintDocument()
        {
            return CreatePrintDocument(DjVuPrintMode.CutMargin);
        }

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the DjVu document.
        /// </summary>
        /// <param name="printMode">Specifies the mode for printing. The default
        /// value for this parameter is CutMargin.</param>
        /// <returns></returns>
        public PrintDocument CreatePrintDocument(DjVuPrintMode printMode)
        {
            return CreatePrintDocument(new DjVuPrintSettings(printMode, null));
        }

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the DjVu document.
        /// </summary>
        /// <param name="settings">The settings used to configure the print document.</param>
        /// <returns></returns>
        public PrintDocument CreatePrintDocument(DjVuPrintSettings settings)
        {
            return new DjVuPrintDocument(this, settings);
        }

        /// <summary>
        /// Returns all links on the DjVu page.
        /// </summary>
        /// <param name="page">The page to get the links for.</param>
        /// <param name="size">The size of the page.</param>
        /// <returns>A collection with the links on the page.</returns>
        public DjVuPageLinks GetPageLinks(int page, Size size)
        {
            return _file.GetPageLinks(page, size);
        }

        /// <summary>
        /// Get the current rotation of the page.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public DjVuRotation GetPageRotation(int page)
        {
            return _file.GetPageRotation(page);
        }

        /// <summary>
        /// Rotate the page.
        /// </summary>
        /// <param name="page">The page to rotate.</param>
        /// <param name="rotation">How to rotate the page.</param>
        public void RotatePage(int page, DjVuRotation rotation)
        {
            _file.RotatePage(page, rotation);
            _pageSizes[page] = _file.GetDjVuPageSize(page);
            PageSizes = new ReadOnlyCollection<SizeF>(_pageSizes);
        }

        /// <summary>
        /// Get metadata information from the DjVu document.
        /// </summary>
        /// <returns>The DjVu metadata.</returns>
        public DjVuInformation GetInformation()
        {
            return _file.GetInformation();
        }

        public SizeF GetPageSize(int pageNo)
        {
            if (_pageSizes.Count > pageNo && pageNo >= 0)
            {
                if (_pageSizes[pageNo].IsEmpty)
                    _pageSizes[pageNo] = _file.GetDjVuPageSize(pageNo);

                return _pageSizes[pageNo];
            }

            return _pageSizes[0];
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <param name="disposing">Whether this method is called from Dispose.</param>
        protected void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_file != null)
                {
                    _file.Dispose();
                    _file = null;
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Get detailed information all characters on the page.
        /// </summary>
        /// <param name="page">The page to get the information for.</param>
        /// <returns>The character information.</returns>
        public IList<DjVuCharacterInformation> GetCharacterInformation(int page)
        {
            return _file.GetCharacterInformation(page);
        }
    }
}
