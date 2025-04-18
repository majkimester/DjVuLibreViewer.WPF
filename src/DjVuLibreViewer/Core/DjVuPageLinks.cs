using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace DjVuLibreViewer.Core
{
    /// <summary>
    /// Describes all links on a page.
    /// </summary>
    public class DjVuPageLinks
    {
        /// <summary>
        /// All links of the page.
        /// </summary>
        public IList<DjVuPageLink> Links { get; private set; }

        /// <summary>
        /// Creates a new instance of the DjVuPageLinks class.
        /// </summary>
        /// <param name="links">The links on the DjVu page.</param>
        public DjVuPageLinks(IList<DjVuPageLink> links)
        {
            if (links == null)
                throw new ArgumentNullException(nameof(links));

            Links = new ReadOnlyCollection<DjVuPageLink>(links);
        }

        public DjVuPageLink GetLinkOnLocation(PointF DjVuLocation)
        {
            if (Links != null)
            {
                foreach(var link in Links)
                {
                    if (link.Bounds.Contains(DjVuLocation)) return link;
                }
            }
            return null;
        }
    }
}
