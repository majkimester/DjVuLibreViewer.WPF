using System.Windows.Media;

namespace DjVuLibreViewer.Core
{
    /// <summary>
    /// Represents a marker on a DjVu page.
    /// </summary>
    public interface IDjVuMarker
    {
        /// <summary>
        /// The page where the marker is drawn on.
        /// </summary>
        int Page { get; }

        /// <summary>
        /// Draw the marker.
        /// </summary>
        /// <param name="renderer">The DjVuRenderer to draw the marker with.</param>
        /// <param name="graphics">The Graphics to draw the marker with.</param>
        void Draw(DjVuRenderer renderer, DrawingContext graphics);
    }
}
