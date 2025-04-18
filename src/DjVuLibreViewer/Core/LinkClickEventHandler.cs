using System.ComponentModel;

namespace DjVuLibreViewer.Core
{
    public class LinkClickEventArgs : HandledEventArgs
    {
        /// <summary>
        /// Gets the link that was clicked.
        /// </summary>
        public DjVuPageLink Link { get; private set; }
        
        public LinkClickEventArgs(DjVuPageLink link)
        {
            Link = link;
        }
    }

    public delegate void LinkClickEventHandler(object sender, LinkClickEventArgs e);
}
