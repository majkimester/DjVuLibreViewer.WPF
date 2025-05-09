﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using DjVuLibreViewer.Drawing;
using Color = System.Windows.Media.Color;
using SystemColors = System.Windows.SystemColors;

namespace DjVuLibreViewer.Core
{
    /// <summary>
    /// Helper class for searching through DjVu documents.
    /// </summary>
    public class DjVuSearchManager
    {
        private bool _highlightAllMatches;
        private List<IList<DjVuRectangle>> _bounds;
        private int _firstMatch;
        private int _offset;

        /// <summary>
        /// The renderer associated with the search manager.
        /// </summary>
        public DjVuRenderer Renderer { get; }

        /// <summary>
        /// Gets or sets whether to match case.
        /// </summary>
        public bool MatchCase { get; set; }

        /// <summary>
        /// Gets or sets whether to match whole words.
        /// </summary>
        public bool MatchWholeWord { get; set; }

        /// <summary>
        /// Gets or sets the color of matched search terms.
        /// </summary>
        public Color MatchColor { get; }

        /// <summary>
        /// Gets or sets the border color of matched search terms.
        /// </summary>
        public Color MatchBorderColor { get; }

        /// <summary>
        /// Gets or sets the border width of matched search terms.
        /// </summary>
        public float MatchBorderWidth { get; }

        /// <summary>
        /// Gets or sets the color of the current match.
        /// </summary>
        public Color CurrentMatchColor { get; }

        /// <summary>
        /// Gets or sets the border color of the current match.
        /// </summary>
        public Color CurrentMatchBorderColor { get; }

        /// <summary>
        /// Gets or sets the border width of the current match.
        /// </summary>
        public float CurrentMatchBorderWidth { get; }

        public int MatchesCount => _bounds?.Count ?? 0;

        public int MatchesOffset => _offset;

        /// <summary>
        /// Gets or sets whether all matches should be highlighted.
        /// </summary>
        public bool HighlightAllMatches
        {
            get => _highlightAllMatches;
            set
            {
                if (_highlightAllMatches != value)
                {
                    _highlightAllMatches = value;
                    UpdateHighlights();
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the search manager.
        /// </summary>
        /// <param name="renderer">The renderer to create the search manager for.</param>
        public DjVuSearchManager(DjVuRenderer renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

            HighlightAllMatches = true;
            MatchColor = Colors.Yellow;
            CurrentMatchColor = SystemColors.HighlightColor;
        }

        /// <summary>
        /// Searches for the specified text.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <returns>Whether any matches were found.</returns>
        public bool Search(string text)
        {
            Renderer.Markers.Clear();
            _offset = -1;
            _bounds = null;

            if (Renderer.Document != null && !string.IsNullOrEmpty(text))
            {
                DjVuMatches matches = Renderer.Document.Search(text, MatchCase, MatchWholeWord);
                _bounds = GetAllBounds(matches);
                _bounds = _bounds.OrderBy(b => b[0].Page).ThenByDescending(b => b[0].Bounds.Y).ThenBy(b => b[0].Bounds.Y).ToList();
            }

            UpdateHighlights();

            return _bounds != null && _bounds.Count > 0;
        }

        private List<IList<DjVuRectangle>> GetAllBounds(DjVuMatches matches)
        {
            var result = new List<IList<DjVuRectangle>>();

            foreach (var match in matches.Items)
            {
                result.Add(Renderer.Document.GetTextBounds(match.TextSpan));
            }

            return result;
        }

        /// <summary>
        /// Find the next matched term.
        /// </summary>
        /// <param name="forward">Whether or not to search forward.</param>
        /// <returns>False when the first match was found again; otherwise true.</returns>
        public bool FindNext(bool forward)
        {
            if (_bounds == null || _bounds.Count == 0)
                return false;

            if (_offset == -1)
            {
                _offset = FindFirstFromCurrentPage();
                _firstMatch = _offset;

                UpdateHighlights();
                return true;
            }

            if (forward)
            {
                _offset++;
                if (_offset >= _bounds.Count)
                    _offset = 0;
            }
            else
            {
                _offset--;
                if (_offset < 0)
                    _offset = _bounds.Count - 1;
            }

            UpdateHighlights();

            return _offset != _firstMatch;
        }

        private void ScrollCurrentIntoView()
        {
            if (_offset == -1) return;
            var current = _bounds[_offset];
            if (current.Count > 0)
            {
                int page = current[0].Page;
                // Go to the page only if not visible
                if (page < Renderer.PageNo || page > Renderer.PageNoLast)
                {
                    Renderer.GotoPage(current[0].Page);
                }
                Renderer.ScrollIntoView(current[0]);
            }
        }

        private int FindFirstFromCurrentPage()
        {
            for (int i = 0; i < Renderer.Document.PageCount; i++)
            {
                int page = (i + Renderer.PageNo) % Renderer.Document.PageCount;

                for (int j = 0; j < _bounds.Count; j++)
                {
                    var bound = _bounds[j];
                    if (bound[0].Page == page)
                        return j;
                }
            }

            return 0;
        }

        /// <summary>
        /// Resets the search manager.
        /// </summary>
        public void Reset()
        {
            Search(null);
        }

        private void UpdateHighlights()
        {
            Renderer.Markers.Clear();

            if (_bounds == null)
            {
                Renderer.RedrawMarkers();
                return;
            }

            if (_highlightAllMatches)
            {
                for (int i = 0; i < _bounds.Count; i++)
                {
                    bool current = _offset == -1 ? false : i == _offset;
                    AddMatch(i, current);
                }
            }
            else if (_offset != -1)
            {
                AddMatch(_offset, true);
            }
            ScrollCurrentIntoView();
            Renderer.RedrawMarkers();
        }

        private void AddMatch(int index, bool current)
        {
            foreach (var DjVuBounds in _bounds[index])
            {
                var bounds = new RectangleF(
                    DjVuBounds.Bounds.Left - 1,
                    DjVuBounds.Bounds.Top + 1,
                    DjVuBounds.Bounds.Width + 2,
                    DjVuBounds.Bounds.Height - 2
                );

                var marker = new DjVuMarker(
                    DjVuBounds.Page,
                    bounds,
                    current ? CurrentMatchColor : MatchColor,
                    current ? CurrentMatchBorderColor : MatchBorderColor,
                    current ? CurrentMatchBorderWidth : MatchBorderWidth
                );

                Renderer.Markers.Add(marker);
            }
        }
    }
}
