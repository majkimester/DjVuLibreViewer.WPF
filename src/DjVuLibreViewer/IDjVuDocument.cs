using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Media;

using DjVuLibreViewer.Core;
using DjVuLibreViewer.Drawing;
using DjVuLibreViewer.Enums;


namespace DjVuLibreViewer
{
    /// <summary>
    /// Represents a DjVu document.
    /// </summary>
    public interface IDjVuDocument : IDisposable
    {
        /// <summary>
        /// Number of pages in the DjVu document.
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// Bookmarks stored in this DjVuFile
        /// </summary>
        DjVuBookmarkCollection Bookmarks { get; }

        /// <summary>
        /// Size of each page in the DjVu document.
        /// </summary>
        IList<SizeF> PageSizes { get; }

        /// <summary>
        /// Renders a page of the DjVu document to the provided graphics instance.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="graphics">Graphics instance to render the page on.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="bounds">Bounds to render the page in.</param>
        void RenderToGraphics(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds);

        /// <summary>
        /// Renders a page of the DjVu document to an image.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="width">Width of the rendered image.</param>
        /// <param name="height">Height of the rendered image.</param>
        /// <param name="rotate">Rotation.</param>
        /// <returns>The rendered image.</returns>
        ImageSource Render(int page, int width, int height, DjVuRotation rotate);

        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <returns>All matches.</returns>
        DjVuMatches Search(string text, bool matchCase, bool wholeWord);

        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <param name="page">The page to search on.</param>
        /// <returns>All matches.</returns>
        DjVuMatches Search(string text, bool matchCase, bool wholeWord, int page);

        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <param name="startPage">The page to start searching.</param>
        /// <param name="endPage">The page to end searching.</param>
        /// <returns>All matches.</returns>
        DjVuMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage);

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the DjVu document.
        /// </summary>
        /// <returns></returns>
        PrintDocument CreatePrintDocument();

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the DjVu document.
        /// </summary>
        /// <param name="printMode">Specifies the mode for printing. The default
        /// value for this parameter is CutMargin.</param>
        /// <returns></returns>
        PrintDocument CreatePrintDocument(DjVuPrintMode printMode);

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the DjVu document.
        /// </summary>
        /// <param name="settings">The settings used to configure the print document.</param>
        /// <returns></returns>
        PrintDocument CreatePrintDocument(DjVuPrintSettings settings);

        /// <summary>
        /// Returns all links on the DjVu page.
        /// </summary>
        /// <param name="page">The page to get the links for.</param>
        /// <param name="size">The size of the page.</param>
        /// <returns>A collection with the links on the page.</returns>
        DjVuPageLinks GetPageLinks(int page, Size size);
       
        /// <summary>
        /// Get metadata information from the DjVu document.
        /// </summary>
        /// <returns>The DjVu metadata.</returns>
        DjVuInformation GetInformation();

        /// <summary>
        /// Get all text on the page.
        /// </summary>
        /// <param name="page">The page to get the text for.</param>
        /// <returns>The text on the page.</returns>
        string GetDjVuText(int page);

        /// <summary>
        /// Get all text matching the text span.
        /// </summary>
        /// <param name="textSpan">The span to get the text for.</param>
        /// <returns>The text matching the span.</returns>
        string GetDjVuText(DjVuTextSpan textSpan);

        /// <summary>
        /// Get all bounding rectangles for the text span.
        /// </summary>
        /// <description>
        /// The algorithm used to get the bounding rectangles tries to join
        /// adjacent character bounds into larger rectangles.
        /// </description>
        /// <param name="textSpan">The span to get the bounding rectangles for.</param>
        /// <returns>The bounding rectangles.</returns>
        IList<DjVuRectangle> GetTextBounds(DjVuTextSpan textSpan);

        /// <summary>
        /// Convert a point from device coordinates to page coordinates.
        /// </summary>
        /// <param name="page">The page number where the point is from.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns>The converted point.</returns>
        PointF PointToDjVu(int page, Point point);

        /// <summary>
        /// Convert a point from page coordinates to device coordinates.
        /// </summary>
        /// <param name="page">The page number where the point is from.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns>The converted point.</returns>
        Point PointFromDjVu(int page, PointF point);

        /// <summary>
        /// Convert a rectangle from device coordinates to page coordinates.
        /// </summary>
        /// <param name="page">The page where the rectangle is from.</param>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>The converted rectangle.</returns>
        RectangleF RectangleToDjVu(int page, Rectangle rect);

        /// <summary>
        /// Convert a rectangle from page coordinates to device coordinates.
        /// </summary>
        /// <param name="page">The page where the rectangle is from.</param>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>The converted rectangle.</returns>
        Rectangle RectangleFromDjVu(int page, RectangleF rect);

        /// Get detailed information for all characters on the page.
        /// </summary>
        /// <param name="page">The page to get the information for.</param>
        /// <returns>The character information.</returns>
        IList<DjVuCharacterInformation> GetCharacterInformation(int page);

        /// <summary>
        /// Get the character index at or nearby a specific position. 
        /// </summary>
        /// <param name="location">The location to inspect</param>
        /// <param name="xTolerance">An x-axis tolerance value for character hit detection, in point unit.</param>
        /// <param name="yTolerance">A y-axis tolerance value for character hit detection, in point unit.</param>
        /// <returns>The zero-based index of the character at, or nearby the point specified by parameter x and y. If there is no character at or nearby the point, it will return -1.</returns>
        int GetCharacterIndexAtPosition(DjVuPoint location, double xTolerance, double yTolerance);

        /// <summary>
        /// Get the full word at or nearby a specific position
        /// </summary>
        /// <param name="location">The location to inspect</param>
        /// <param name="xTolerance">An x-axis tolerance value for character hit detection, in point unit.</param>
        /// <param name="yTolerance">A y-axis tolerance value for character hit detection, in point unit.</param>
        /// <param name="span">The location of the found word, if any</param>
        /// <returns>A value indicating whether a word was found at the specified location</returns>
        bool GetWordAtPosition(DjVuPoint location, double xTolerance, double yTolerance, out DjVuTextSpan span);

        /// <summary>
        /// Get number of characters in a page.
        /// </summary>
        /// <param name="page">The page to get the character count from</param>
        /// <returns>Number of characters in the page. Generated characters, like additional space characters, new line characters, are also counted.</returns>
        int CountCharacters(int page);

        /// <summary>
        /// Gets the rectangular areas occupied by a segment of text
        /// </summary>
        /// <param name="page">The page to get the rectangles from</param>
        /// <returns>The rectangular areas occupied by a segment of text</returns>
        List<DjVuRectangle> GetTextRectangles(int page, int startIndex, int count);
    }
}
