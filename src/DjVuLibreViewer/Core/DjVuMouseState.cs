using DjVuLibreViewer.Drawing;
using System.Windows;

namespace DjVuLibreViewer.Core
{
    public class DjVuMouseState
    {
        public Point MouseLocation { get; set; }
        public DjVuPoint DjVuLocation { get; set; }
        public int CharacterIndex { get; set; }
    }
}
