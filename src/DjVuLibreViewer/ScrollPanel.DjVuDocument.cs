using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Documents;
using DjVuLibreViewer.Core;
using DjVuLibreViewer.Drawing;
using DjVuLibreViewer.Enums;

namespace DjVuLibreViewer
{
    // ScrollPanel.DjVuDocument
    public partial class ScrollPanel
    {
        public void Render(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds)
        {
            Document.Render(page, graphics, dpiX, dpiY, bounds);
        }

        public Image Render(int page, float dpiX, float dpiY)
        {
            return Document.Render(page, dpiX, dpiY);
        }

        public Image Render(int page, int width, int height, float dpiX, float dpiY)
        {
            return Document.Render(page, width, height, dpiX, dpiY);
        }

        public Image Render(int page, int width, int height, float dpiX, float dpiY, DjVuRotation rotate)
        {
            return Document.Render(page, width, height, dpiX, dpiY, rotate);
        }

        public DjVuMatches Search(string text, bool matchCase, bool wholeWord)
        {
            return Document?.Search(text, matchCase, wholeWord);
        }

        public DjVuMatches Search(string text, bool matchCase, bool wholeWord, int page)
        {
            return Document?.Search(text, matchCase, wholeWord, page);
        }

        public DjVuMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage)
        {
            return Document?.Search(text, matchCase, wholeWord, startPage, endPage);
        }

        public PrintDocument CreatePrintDocument()
        {
            return Document.CreatePrintDocument();
        }

        public PrintDocument CreatePrintDocument(DjVuPrintMode printMode)
        {
            return Document.CreatePrintDocument(printMode);
        }

        public PrintDocument CreatePrintDocument(DjVuPrintSettings settings)
        {
            return Document.CreatePrintDocument(settings);
        }

        public DjVuPageLinks GetPageLinks(int page, Size size)
        {
            return Document.GetPageLinks(page, size);
        }

        public void RotatePage(int page, DjVuRotation rotate)
        {
            Rotate = rotate;
            OnPagesDisplayModeChanged();
        }

        public DjVuInformation GetInformation()
        {
            return Document?.GetInformation();
        }

        public string GetDjVuText(int page)
        {
            return Document?.GetDjVuText(page);
        }

        public string GetDjVuText(DjVuTextSpan textSpan)
        {
            return Document?.GetDjVuText(textSpan);
        }

        public IList<DjVuRectangle> GetTextBounds(DjVuTextSpan textSpan)
        {
            return Document?.GetTextBounds(textSpan);
        }

        public PointF PointToDjVu(int page, Point point)
        {
            return Document.PointToDjVu(page, point);
        }

        public Point PointFromDjVu(int page, PointF point)
        {
            return Document.PointFromDjVu(page, point);
        }

        public RectangleF RectangleToDjVu(int page, Rectangle rect)
        {
            return Document.RectangleToDjVu(page, rect);
        }

        public Rectangle RectangleFromDjVu(int page, RectangleF rect)
        {
            return Document.RectangleFromDjVu(page, rect);
        }

        public void GotoPage(int page, bool forceRender=false)
        {
            if (IsDocumentLoaded)
            {
                PageNo = page;
                PageNoLast = page;

                // ContinuousMode will be rendered in OnScrollChanged
                if (PagesDisplayMode != DjVuPagesDisplayMode.ContinuousMode || forceRender)
                {
                    CurrentPageSize = CalculatePageSize(page);
                    RenderPage(Frame1, page, CurrentPageSize.Width, CurrentPageSize.Height);
                    Frame1.AddAdorner();

                    if (PagesDisplayMode == DjVuPagesDisplayMode.BookMode && page + 1 < Document.PageCount)
                    {
                        var nextPageSize = CalculatePageSize(page + 1);
                        RenderPage(Frame2, page + 1, nextPageSize.Width, nextPageSize.Height);
                        Frame2.AddAdorner();
                        PageNoLast = page + 1;
                    }

                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                    layer?.Update();
                }
                ScrollToPage(PageNo);
            }
        }

        public void NextPage()
        {
            if (IsDocumentLoaded)
            {
                var extentVal = PagesDisplayMode == DjVuPagesDisplayMode.BookMode ? 2 : 1;
                GotoPage(Math.Min(Math.Max(PageNo + extentVal, 0), PageCount - extentVal));
            }
        }

        public void PreviousPage()
        {
            if (IsDocumentLoaded)
            {
                var extentVal = PagesDisplayMode == DjVuPagesDisplayMode.BookMode ? 2 : 1;
                GotoPage(Math.Min(Math.Max(PageNo - extentVal, 0), PageCount - extentVal));
            }
        }
    }
}
