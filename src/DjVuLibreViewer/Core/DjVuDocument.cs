using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Media;

using DjVuLibreViewer.Drawing;
using DjVuLibreViewer.Enums;


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
        public void RenderToGraphics(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            graphics.PageUnit = GraphicsUnit.Pixel;
            Bitmap bitmap = _file.RenderDjVuPageToBitmap(page, bounds.Width, bounds.Height, (int)dpiX, (int)dpiY, DjVuRotation.Rotate0);
            graphics.DrawImage(bitmap, bounds);
            bitmap.Dispose();
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
        public Bitmap RenderToBitmap(int page, int width, int height, float dpiX, float dpiY, DjVuRotation rotate)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            return _file.RenderDjVuPageToBitmap(page, width, height, (int)dpiX, (int)dpiY, rotate);
        }

        /// <summary>
        /// Renders a page of the DjVu document to an image.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="width">Width of the rendered image.</param>
        /// <param name="height">Height of the rendered image.</param>
        /// <param name="rotate">Rotation.</param>
        /// <returns>The rendered image.</returns>
        public ImageSource Render(int page, int width, int height, DjVuRotation rotate)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            return _file.RenderDjVuPage(page, width, height, rotate);
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
