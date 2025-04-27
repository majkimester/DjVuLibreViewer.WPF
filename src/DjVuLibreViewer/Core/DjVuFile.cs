using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

using DjvuSharp;
using DjvuSharp.Rendering;

using DjVuLibreViewer.Drawing;
using DjVuLibreViewer.Enums;
using DjVuLibreViewer.Helpers;
using System.Windows.Media.Imaging;
using DjvuSharp.Enums;

namespace DjVuLibreViewer.Core
{
    internal class DjVuFile : IDisposable
    {
        DjvuDocument _document;
        private bool _disposed;

        private long _DjVuFileSize = 0;
        private string _DjVuFilePath = null;

        public DjVuBookmarkCollection Bookmarks { get; private set; }

        public DjVuFile(string djvuFile)
        {
            if (djvuFile == null)
                throw new ArgumentNullException(nameof(djvuFile));

            _DjVuFileSize = new FileInfo(djvuFile).Length;
            _DjVuFilePath = djvuFile;

            _document = DjvuDocument.Create(djvuFile);

            Bookmarks = new DjVuBookmarkCollection();
            LoadBookmarks(Bookmarks);
        }

        public Bitmap RenderDjVuPageToBitmap(int pageNumber, int width, int height, int dpiX, int dpiY, DjVuRotation rotate)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            using (var renderEngine = RenderEngineFactory.CreateRenderEngine(PixelFormatStyle.RGB24))
            {
                renderEngine.SetRowOrder(true);
                DjvuPage page = new DjvuPage(_document, pageNumber);
                width = width * dpiX / page.Resolution;
                height = height * dpiX / page.Resolution;
                var pageRect = new DjvuSharp.Rectangle(0, 0, width, height);
                var renderRect = new DjvuSharp.Rectangle(0, 0, width, height);
                byte[] imagePixels = renderEngine.RenderPage(page, RenderMode.COLOR, pageRect, renderRect);
                return BitmapHelper.CreateBitmapFromRgb24(imagePixels, width, height, dpiX, dpiY);
            }
        }

        public BitmapSource RenderDjVuPage(int pageNumber, int width, int height, DjVuRotation rotate)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            using (var renderEngine = RenderEngineFactory.CreateRenderEngine(PixelFormatStyle.RGB24))
            {
                renderEngine.SetRowOrder(true);
                DjvuPage page = new DjvuPage(_document, pageNumber);
                page.Rotation = (PageRotation)rotate;
                // Limit maximum render size
                if (width > page.Width)
                {
                    width = page.Width;
                    height = page.Height;
                }
                var pageRect = new DjvuSharp.Rectangle(0, 0, width, height);
                var renderRect = new DjvuSharp.Rectangle(0, 0, width, height);
                byte[] imagePixels = renderEngine.RenderPage(page, RenderMode.COLOR, pageRect, renderRect);
                return BitmapHelper.CreateFromRgb24(imagePixels, width, height, 96, 96);
            }
        }

        public int GetPageCount()
        {
            return _document.PageNumber;
        }

        public DjVuPageLinks GetPageLinks(int pageNumber, Size pageSize)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var links = new List<DjVuPageLink>();

            /*
            int link = 0;
            IntPtr annotation;

            while (NativeMethods.FPDFLink_Enumerate(GetPageData(pageNumber).Page, ref link, out annotation))
            {
                var destination = NativeMethods.FPDFLink_GetDest(_document, annotation);
                int? target = null;
                string uri = null;

                if (destination != IntPtr.Zero)
                    target = (int)NativeMethods.FPDFDest_GetDestPageIndex(_document, destination);

                var action = NativeMethods.FPDFLink_GetAction(annotation);
                if (action != IntPtr.Zero)
                {
                    const uint length = 1024;
                    var sb = new StringBuilder(1024);
                    NativeMethods.FPDFAction_GetURIPath(_document, action, sb, length);

                    uri = sb.ToString();
                }

                var rect = new NativeMethods.FS_RECTF();

                if (NativeMethods.FPDFLink_GetAnnotRect(annotation, rect) && (target.HasValue || !string.IsNullOrEmpty(uri)))
                {
                    links.Add(new DjVuPageLink(
                        new RectangleF(rect.left, rect.bottom, rect.right - rect.left, rect.top - rect.bottom),
                        target,
                        uri
                    ));
                }
            }
            */

            // TODO

            return new DjVuPageLinks(links);
        }

        public List<SizeF> GetDjVuDocInfo()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            int pageCount = _document.PageNumber;
            var result = new List<SizeF>(pageCount);

            for (int i = 0; i < pageCount; i++)
            {
                result.Add(GetDjVuPageSize(i));
            }

            return result;
        }

        public SizeF GetDjVuPageSize(int pageNumber)
        {
            DjvuPage page = new DjvuPage(_document, pageNumber);
            return new SizeF(page.Width, page.Height);
        }

        private void LoadBookmarks(DjVuBookmarkCollection bookmarks)
        {
            // TODO load bookmarks
        }

        public DjVuMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage)
        {
            var matches = new List<DjVuMatch>();

            /* TODO
            if (String.IsNullOrEmpty(text))
                return new DjVuMatches(startPage, endPage, matches);

            for (int page = startPage; page <= endPage; page++)
            {
                var pageData = GetPageData(page);

                NativeMethods.FPDF_SEARCH_FLAGS flags = 0;
                if (matchCase)
                    flags |= NativeMethods.FPDF_SEARCH_FLAGS.FPDF_MATCHCASE;
                if (wholeWord)
                    flags |= NativeMethods.FPDF_SEARCH_FLAGS.FPDF_MATCHWHOLEWORD;

                var handle = NativeMethods.FPDFText_FindStart(pageData.TextPage, FDjVuEncoding.GetBytes(text), flags, 0);

                try
                {
                    while (NativeMethods.FPDFText_FindNext(handle))
                    {
                        int index = NativeMethods.FPDFText_GetSchResultIndex(handle);

                        int matchLength = NativeMethods.FPDFText_GetSchCount(handle);

                        var result = new byte[(matchLength + 1) * 2];
                        NativeMethods.FPDFText_GetText(pageData.TextPage, index, matchLength, result);
                        string match = FDjVuEncoding.GetString(result, 0, matchLength * 2);

                        matches.Add(new DjVuMatch(
                            match,
                            new DjVuTextSpan(page, index, matchLength),
                            page
                        ));
                    }
                }
                finally
                {
                    NativeMethods.FPDFText_FindClose(handle);
                }
            }
            */

            return new DjVuMatches(startPage, endPage, matches);
        }

        public IList<DjVuRectangle> GetTextBounds(DjVuTextSpan textSpan)
        {
            // TODO
            //return GetTextBounds(GetPageData(textSpan.Page).TextPage, textSpan.Page, textSpan.Offset, textSpan.Length);

            return new List<DjVuRectangle>();
        }

        public Point PointFromDjVu(int page, PointF point)
        {
            /* TODO
            var pageData = GetPageData(page);
            NativeMethods.FPDF_PageToDevice(
                pageData.Page,
                0,
                0,
                (int)pageData.Width,
                (int)pageData.Height,
                0,
                point.X,
                point.Y,
                out var deviceX,
                out var deviceY
            );

            return new Point(deviceX, deviceY);
            */
            return new Point((int)point.X, (int)point.Y);
        }

        public System.Drawing.Rectangle RectangleFromDjVu(int page, RectangleF rect)
        {
            /* TODO
            var pageData = GetPageData(page);
            NativeMethods.FPDF_PageToDevice(
                pageData.Page,
                0,
                0,
                (int)pageData.Width,
                (int)pageData.Height,
                0,
                rect.Left,
                rect.Top,
                out var deviceX1,
                out var deviceY1
            );

            NativeMethods.FPDF_PageToDevice(
                pageData.Page,
                0,
                0,
                (int)pageData.Width,
                (int)pageData.Height,
                0,
                rect.Right,
                rect.Bottom,
                out var deviceX2,
                out var deviceY2
            );

            return new Rectangle(
                deviceX1,
                deviceY1,
                deviceX2 - deviceX1,
                deviceY2 - deviceY1
            );
            */

            return new System.Drawing.Rectangle();
        }

        public PointF PointToDjVu(int page, Point point)
        {
            /* TODO
            var pageData = GetPageData(page);
            NativeMethods.FPDF_DeviceToPage(
                pageData.Page,
                0,
                0,
                (int)pageData.Width,
                (int)pageData.Height,
                0,
                point.X,
                point.Y,
                out var deviceX,
                out var deviceY
            );

            return new PointF((float)deviceX, (float)deviceY);
            */
            return new PointF((float)point.X, (float)point.Y);
        }

        public RectangleF RectangleToDjVu(int page, System.Drawing.Rectangle rect)
        {
            /* TODO
            var pageData = GetPageData(page);
            NativeMethods.FPDF_DeviceToPage(
                pageData.Page,
                0,
                0,
                (int)pageData.Width,
                (int)pageData.Height,
                0,
                rect.Left,
                rect.Top,
                out var deviceX1,
                out var deviceY1
            );

            NativeMethods.FPDF_DeviceToPage(
                pageData.Page,
                0,
                0,
                (int)pageData.Width,
                (int)pageData.Height,
                0,
                rect.Right,
                rect.Bottom,
                out var deviceX2,
                out var deviceY2
            );

            return new RectangleF(
                (float)deviceX1,
                (float)deviceY1,
                (float)(deviceX2 - deviceX1),
                (float)(deviceY2 - deviceY1)
            );
            */

            return new RectangleF();
        }

        private IList<DjVuRectangle> GetTextBounds(IntPtr textPage, int page, int index, int matchLength)
        {
            var result = new List<DjVuRectangle>();
            RectangleF? lastBounds = null;

            for (int i = 0; i < matchLength; i++)
            {
                var bounds = GetBounds(textPage, index + i);

                if (bounds.Width == 0 || bounds.Height == 0)
                    continue;

                if (
                    lastBounds.HasValue &&
                    AreClose(lastBounds.Value.Right, bounds.Left) &&
                    AreClose(lastBounds.Value.Top, bounds.Top) &&
                    AreClose(lastBounds.Value.Bottom, bounds.Bottom)
                )
                {
                    float top = Math.Max(lastBounds.Value.Top, bounds.Top);
                    float bottom = Math.Min(lastBounds.Value.Bottom, bounds.Bottom);

                    lastBounds = new RectangleF(
                        lastBounds.Value.Left,
                        top,
                        bounds.Right - lastBounds.Value.Left,
                        bottom - top
                    );

                    result[result.Count - 1] = new DjVuRectangle(page, lastBounds.Value);
                }
                else
                {
                    lastBounds = bounds;
                    result.Add(new DjVuRectangle(page, bounds));
                }
            }

            return result;
        }

        private bool AreClose(float p1, float p2)
        {
            return Math.Abs(p1 - p2) < 4f;
        }

        private RectangleF GetBounds(IntPtr textPage, int index)
        {
            /* TODO
            NativeMethods.FPDFText_GetCharBox(
                textPage,
                index,
                out var left,
                out var right,
                out var bottom,
                out var top
            );

            return new RectangleF(
                (float)left,
                (float)top,
                (float)(right - left),
                (float)(bottom - top)
            );
            */

            return new RectangleF();
        }

        public string GetDjVuText(int pageNumber)
        {
            DjvuPage page = new DjvuPage(_document, pageNumber);
            return page.GetPageFullText();
        }

        public string GetDjVuText(DjVuTextSpan textSpan)
        {
            //return GetDjVuText(GetPageData(textSpan.Page), textSpan);
            return "xyz";
        }

        /* TODO
        private string GetDjVuText(PageData pageData, DjVuTextSpan textSpan)
        {
            var result = new byte[(textSpan.Length + 1) * 2];
            int count = NativeMethods.FPDFText_GetText(pageData.TextPage, textSpan.Offset, textSpan.Length, result);
            if (count <= 0)
                return string.Empty;
            return FDjVuEncoding.GetString(result, 0, (count - 1) * 2);
        }
    */

        public IList<DjVuCharacterInformation> GetCharacterInformation(int page)
        {
            /* TODO
            using (var pageData = new PageData(_document, _form, page))
            {
                var result = new List<DjVuCharacterInformation>();
                int charCount = NativeMethods.FPDFText_CountChars(pageData.TextPage);
                var allChars = GetDjVuText(pageData, new DjVuTextSpan(page, 0, charCount)).ToCharArray();

                for (int i = 0; i < charCount; i++)
                {
                    var bounds = GetBounds(pageData.TextPage, i);
                    double fontSize = NativeMethods.FPDFText_GetFontSize(pageData.TextPage, i);
                    result.Add(new DjVuCharacterInformation(page, i, allChars[i], fontSize, bounds));
                }

                return result;
            }
            */
            return new List<DjVuCharacterInformation>();
        }

        public int GetCharIndexAtPos(DjVuPoint location, double xTolerance, double yTolerance)
        {
            /* TODO
            return NativeMethods.FPDFText_GetCharIndexAtPos(
                GetPageData(location.Page).TextPage,
                location.Location.X,
                location.Location.Y,
                xTolerance,
                yTolerance
            );
            */
            return 0;
        }

        public bool GetWordAtPosition(DjVuPoint location, double xTolerance, double yTolerance, out DjVuTextSpan span)
        {
            var index = GetCharIndexAtPos(location, xTolerance, yTolerance);
            if (index < 0)
            {
                span = default(DjVuTextSpan);
                return false;
            }

            var baseCharacter = GetCharacter(location.Page, index);
            if (IsWordSeparator(baseCharacter))
            {
                span = default(DjVuTextSpan);
                return false;
            }

            int start = index, end = index;

            for (int i = index - 1; i >= 0; i--)
            {
                var c = GetCharacter(location.Page, i);
                if (IsWordSeparator(c))
                    break;
                start = i;
            }

            var count = CountChars(location.Page);
            for (int i = index + 1; i < count; i++)
            {
                var c = GetCharacter(location.Page, i);
                if (IsWordSeparator(c))
                    break;
                end = i;
            }

            span = new DjVuTextSpan(location.Page, start, end - start);
            return true;

            bool IsWordSeparator(char c)
            {
                return char.IsSeparator(c) || char.IsPunctuation(c) || char.IsControl(c) || char.IsWhiteSpace(c) || c == '\r' || c == '\n';
            }
        }

        public char GetCharacter(int page, int index)
        {
            /* TODO
            return NativeMethods.FPDFText_GetUnicode(GetPageData(page).TextPage, index);
            */
            return 'X';
        }

        public int CountChars(int page)
        {
            // TODO return NativeMethods.FPDFText_CountChars(GetPageData(page).TextPage);
            return 0;
        }

        public List<DjVuRectangle> GetTextRectangles(int page, int startIndex, int count)
        {
            // TODO return NativeMethods.FPDFText_GetRectangles(GetPageData(page).TextPage, page, startIndex, count);
            return new List<DjVuRectangle>();
        }

        public DjVuInformation GetInformation()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var DjVuInfo = new DjVuInformation();

            if (_DjVuFilePath != null)
            {
                DjVuInfo.FileName = Path.GetFileName(_DjVuFilePath);
                DjVuInfo.FilePath = Path.GetFullPath(_DjVuFilePath);
            }
            DjVuInfo.FileSize = _DjVuFileSize;

            Annotation annotations = _document.GetAnnotations(true);
            Dictionary<string, string> metadata = annotations?.Metadata;
            if (metadata != null)
            {
                DjVuInfo.Creator = GetMetaText(metadata, "Creator");
                DjVuInfo.Title = GetMetaText(metadata, "Title");
                DjVuInfo.Author = GetMetaText(metadata, "Author");
                DjVuInfo.Subject = GetMetaText(metadata, "Subject");
                DjVuInfo.Keywords = GetMetaText(metadata, "Keywords");
                DjVuInfo.Producer = GetMetaText(metadata, "Producer");
                DjVuInfo.CreationDate = GetMetaTextAsDate(metadata, "CreationDate");
                DjVuInfo.ModificationDate = GetMetaTextAsDate(metadata, "ModDate");
            }

            DjvuPage page = new DjvuPage(_document, 0);
            DjVuInfo.Version = page.Version.ToString();
            DjVuInfo.DocumentType = _document.Type.ToString();
            DjVuInfo.PageCount = _document.PageNumber;

            DjVuInfo.PageWidth = page.Width / page.Resolution * 25.4f;
            DjVuInfo.PageHeight = page.Height / page.Resolution * 25.4f;
            return DjVuInfo;
        }

        private string GetMetaText(Dictionary<string, string> metadata, string tag)
        {
            if (metadata.TryGetValue(tag, out string value))
            {
                return value;
            }
            else if (metadata.TryGetValue(tag.ToLower(), out string value2))
            {
                return value2;
            }
            return string.Empty;
        }

        public DateTime? GetMetaTextAsDate(Dictionary<string, string> metadata, string tag)
        {
            string dt = GetMetaText(metadata, tag);

            if (string.IsNullOrEmpty(dt))
                return null;

            Regex dtRegex =
                new Regex(
                    @"(?:D:)(?<year>\d\d\d\d)(?<month>\d\d)(?<day>\d\d)(?<hour>\d\d)(?<minute>\d\d)(?<second>\d\d)(?<tz_offset>[+-zZ])?(?<tz_hour>\d\d)?'?(?<tz_minute>\d\d)?'?");

            Match match = dtRegex.Match(dt);

            if (match.Success)
            {
                var year = match.Groups["year"].Value;
                var month = match.Groups["month"].Value;
                var day = match.Groups["day"].Value;
                var hour = match.Groups["hour"].Value;
                var minute = match.Groups["minute"].Value;
                var second = match.Groups["second"].Value;
                var tzOffset = match.Groups["tz_offset"]?.Value;
                var tzHour = match.Groups["tz_hour"]?.Value;
                var tzMinute = match.Groups["tz_minute"]?.Value;

                string formattedDate = $"{year}-{month}-{day}T{hour}:{minute}:{second}.0000000";

                if (!string.IsNullOrEmpty(tzOffset))
                {
                    switch (tzOffset)
                    {
                        case "Z":
                        case "z":
                            formattedDate += "+0";
                            break;
                        case "+":
                        case "-":
                            formattedDate += $"{tzOffset}{tzHour}:{tzMinute}";
                            break;
                    }
                }

                try
                {
                    return DateTime.Parse(formattedDate);
                }
                catch (FormatException)
                {
                    return null;
                }
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _document.Dispose();
                _disposed = true;
            }
        }
    }
}
