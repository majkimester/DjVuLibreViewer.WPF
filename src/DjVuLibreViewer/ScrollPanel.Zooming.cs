using DjVuLibreViewer.Enums;
using System;
using System.ComponentModel;

namespace DjVuLibreViewer
{
    public partial class ScrollPanel
    {
        public const double DefaultZoomMin = 0.1;
        public const double DefaultZoomMax = 4;
        public const double DefaultZoomFactor = 1.2;

        /// <summary>
        /// Zoom allowed.
        /// </summary>
        public bool IsZoomAllowed { get; set; } = true;

        /// <summary>
        /// Zoom mode (FitHeight, FitWidth or None)
        /// </summary>
        public DjVuZoomMode ZoomMode {
            get => _zoomMode;
            set
            {
                if (_zoomMode != value)
                {
                    _zoomMode = value;
                    OnPagesDisplayModeChanged();
                    OnPropertyChanged();
                }
            }
        }
        private DjVuZoomMode _zoomMode = DjVuZoomMode.FitHeight;

        /// <summary>
        /// Current zoom level.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(1.0)]
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (IsZoomAllowed)
                {
                    var newZoom = Math.Min(Math.Max(value, ZoomMin), ZoomMax);
                    if (newZoom != _zoom)
                    {
                        _zoom = newZoom;
                        if (_zoomMode != DjVuZoomMode.None)
                        {
                            _zoomMode = DjVuZoomMode.None;
                            OnPropertyChanged(nameof(ZoomMode));
                        }
                        OnPagesDisplayModeChanged();
                        OnPropertyChanged();
                    }
                }
            }
        }
        private double _zoom = 1.0;

        /// <summary>
        /// Maximum zoom level.
        /// </summary>
        [DefaultValue(DefaultZoomMin)] public double ZoomMin { get; set; } = DefaultZoomMin;

        /// <summary>
        /// Minimum zoom level.
        /// </summary>
        [DefaultValue(DefaultZoomMax)] public double ZoomMax { get; set; } = DefaultZoomMax;

        /// <summary>
        /// Zoom step.
        /// </summary>
        [DefaultValue(DefaultZoomFactor)] public double ZoomFactor { get; set; } = DefaultZoomFactor;

        /// <summary>
        /// Zooms the DjVu document in one step.
        /// </summary>
        public void ZoomIn()
        {
            Zoom = Zoom * ZoomFactor;
        }

        /// <summary>
        /// Zooms the DjVu document out one step.
        /// </summary>
        public void ZoomOut()
        {
            Zoom = Zoom / ZoomFactor;
        }
    }
}
