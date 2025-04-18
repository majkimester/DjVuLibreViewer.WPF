using System.Collections.ObjectModel;

namespace DjVuLibreViewer.Core
{
    public class DjVuBookmark
    {
        public string Title { get; set; }
        public int PageIndex { get; set; }

        public DjVuBookmarkCollection Children { get; }

        public DjVuBookmark()
        {
            Children = new DjVuBookmarkCollection();
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public class DjVuBookmarkCollection : Collection<DjVuBookmark>
    {
    }
}
