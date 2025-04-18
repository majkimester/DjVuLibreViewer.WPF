using System.Drawing;

namespace DjVuLibreViewer.Core
{
    /// <summary>
    /// Describes a link on a page.
    /// </summary>
    public class DjVuPageLink
    {
        /// <summary>
        /// The location of the link.
        /// </summary>
        public RectangleF Bounds { get; private set; }

        /// <summary>
        /// The target of the link.
        /// </summary>
        public int? TargetPage { get; private set; }

        /// <summary>
        /// The target URI of the link.
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        /// Creates a new instance of the DjVuPageLink class.
        /// </summary>
        /// <param name="bounds">The location of the link</param>
        /// <param name="targetPage">The target page of the link</param>
        /// <param name="uri">The target URI of the link</param>
        public DjVuPageLink(RectangleF bounds, int? targetPage, string uri)
        {
            Bounds = bounds;
            TargetPage = targetPage;
            Uri = uri;
        }
    }
}
