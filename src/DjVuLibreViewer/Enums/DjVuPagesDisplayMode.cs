using System;

namespace DjVuLibreViewer.Enums
{
    [Flags]
    public enum DjVuPagesDisplayMode
    {
        SinglePageMode = 1,
        BookMode = 2,
        ContinuousMode = 4
    }
}
