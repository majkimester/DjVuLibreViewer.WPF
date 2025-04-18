using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable 1591

namespace DjVuLibreViewer.Core
{
    public class DjVuMatches
    {
        public int StartPage { get; private set; }

        public int EndPage { get; private set; }

        public IList<DjVuMatch> Items { get; private set; }

        public DjVuMatches(int startPage, int endPage, IList<DjVuMatch> matches)
        {
            if (matches == null)
                throw new ArgumentNullException(nameof(matches));

            StartPage = startPage;
            EndPage = endPage;
            Items = new ReadOnlyCollection<DjVuMatch>(matches);
        }
    }
}
