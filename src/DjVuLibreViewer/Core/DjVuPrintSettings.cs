using DjVuLibreViewer.Enums;

namespace DjVuLibreViewer.Core
{
    /// <summary>
    /// Configures the print document.
    /// </summary>
    public class DjVuPrintSettings
    {
        /// <summary>
        /// Gets the mode used to print margins.
        /// </summary>
        public DjVuPrintMode Mode { get; }


        /// <summary>
        /// Gets configuration for printing multiple DjVu pages on a single page.
        /// </summary>
        public DjVuPrintMultiplePages MultiplePages { get; }

        /// <summary>
        /// Creates a new instance of the DjVuPrintSettings class.
        /// </summary>
        /// <param name="mode">The mode used to print margins.</param>
        /// <param name="multiplePages">Configuration for printing multiple DjVu
        /// pages on a single page.</param>
        public DjVuPrintSettings(DjVuPrintMode mode, DjVuPrintMultiplePages multiplePages)
        {
            Mode = mode;
            MultiplePages = multiplePages;
        }
    }
}
