using DjVuLibreViewer.Core;
using DjVuLibreViewer.Drawing;

using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using Size = System.Drawing.Size;
using System;
using System.Diagnostics;

namespace DjVuLibreViewer
{
    public partial class DjVuRenderer
    {
        private bool _isSelectingText = false;
        private bool _isSelectingWord = false;
        private bool _isDragAndDropText = false;
        private DjVuMouseState _cachedMouseState = null;
        private DjVuTextSelectionState _startTextSelectionState = null;

        public DjVuTextSelectionState TextSelectionState { get; set; } = null;
        protected bool MousePanningEnabled { get; set; } = true;
        public bool FollowLinkEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the way the viewer should respond to cursor input
        /// </summary>
        [DefaultValue(DjVuCursorMode.TextSelection)]
        public DjVuCursorMode CursorMode
        {
            get { return _cursorMode; }
            set
            {
                _cursorMode = value;
                MousePanningEnabled = _cursorMode == DjVuCursorMode.Pan;
            }
        }
        private DjVuCursorMode _cursorMode = DjVuCursorMode.TextSelection;

        /// <summary>
        /// Indicates whether the user currently has text selected
        /// </summary>
        public bool IsTextSelected
        {
            get
            {
                var state = TextSelectionState?.GetNormalized();
                if (state == null)
                    return false;

                if (state.EndPage < 0 || state.EndIndex < 0)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Gets the currently selected text
        /// </summary>
        public string SelectedText
        {
            get
            {
                var state = TextSelectionState?.GetNormalized();
                if (state == null)
                    return null;

                var sb = new StringBuilder();
                for (int page = state.StartPage; page <= state.EndPage; page++)
                {
                    int start = 0, end = 0;

                    if (page == state.StartPage)
                        start = state.StartIndex;

                    if (page == state.EndPage)
                        end = (state.EndIndex);
                    else
                        end = Document.CountCharacters(page);

                    if (page != state.StartPage)
                        sb.AppendLine();

                    sb.Append(Document.GetDjVuText(new DjVuTextSpan(page, start, end - start + 1)));
                }

                return sb.ToString();
            }
        }

        public void SelectAll()
        {
            TextSelectionState = new DjVuTextSelectionState()
            {
                StartPage = 0,
                StartIndex = 0,
                EndPage = Document.PageCount - 1,
                EndIndex = Document.CountCharacters(Document.PageCount - 1) - 1
            };

            UpdateAdorner();
        }

        public void SelectCurrentPage()
        {
            TextSelectionState = new DjVuTextSelectionState()
            {
                StartPage = PageNo,
                StartIndex = 0,
                EndPage = PageNo,
                EndIndex = Document.CountCharacters(PageNo) - 1
            };

            UpdateAdorner();
        }

        public void CopySelection()
        {
            var text = SelectedText;
            if (text?.Length > 0)
            {
                try
                {
                    Clipboard.SetText(text);
                }
                catch (Exception)
                {
                    // ignore
                };
            }
        }

        public void DrawTextSelection(DrawingContext graphics, int page, DjVuTextSelectionState state)
        {
            if (state == null || state.EndPage < 0 || state.EndIndex < 0)
                return;

            state = state.GetNormalized();

            if (page >= state.StartPage && page <= state.EndPage)
            {
                int start = 0, end = 0;

                if (page == state.StartPage)
                    start = state.StartIndex;

                if (page == state.EndPage)
                    end = (state.EndIndex + 1);
                else
                    end = Document.CountCharacters(page);

                Geometry geometry = null;
                SolidColorBrush brush = new SolidColorBrush(SystemColors.HighlightColor) { Opacity = .5 };
                foreach (var rectangle in Document.GetTextRectangles(page, start, end - start))
                {
                    Rect? bounds = BoundsFromDjVu(rectangle);
                    if (bounds is Rect rectBounds)
                    {
                        if (geometry == null)
                        {
                            geometry = new RectangleGeometry(rectBounds);
                        }
                        else
                        {
                            var geometry2 = new RectangleGeometry(rectBounds);
                            geometry = Geometry.Combine(geometry, geometry2, GeometryCombineMode.Union, null);
                        }
                    }
                }
                if (geometry != null)
                    graphics.DrawGeometry(brush, null, geometry);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Handled) return;
            bool handled = true;

            switch (e.Key)
            {
                case Key.A:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        SelectAll();
                    break;

                case Key.C:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        CopySelection();
                    break;

                case Key.Insert:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        CopySelection();
                    break;

                default:
                    handled = false;
                    break;
            }
            e.Handled = handled;
        }

        internal bool HandleMouseDownForTextSelection(DjVuImage sender, int page, Size viewSize, Point mouseLocation)
        {
            var DjVuLocation = PointToDjVu(page, viewSize, mouseLocation);
            if (!DjVuLocation.IsValid)
                return false;

            var characterIndex = Document.GetCharacterIndexAtPosition(DjVuLocation, 4f, 4f);

            if (characterIndex >= 0)
            {
                _isDragAndDropText = false;
                if (TextSelectionState?.IsPositionInside(DjVuLocation.Page, characterIndex) == true)
                {
                    // click on existing selection
                    _isDragAndDropText = true;
                }
                else if (Keyboard.Modifiers == ModifierKeys.Shift && TextSelectionState != null)
                {
                    // Extend selection
                    var newSelectionLetter = new DjVuTextSelectionState()
                    {
                        StartPage = DjVuLocation.Page,
                        EndPage = DjVuLocation.Page,
                        StartIndex = characterIndex,
                        EndIndex = characterIndex
                    };
                    TextSelectionState = DjVuTextSelectionState.Merge(_startTextSelectionState, newSelectionLetter);
                    UpdateAdorner();
                }
                else
                {
                    // Start new selection
                    _startTextSelectionState = new DjVuTextSelectionState()
                    {
                        StartPage = DjVuLocation.Page,
                        StartIndex = characterIndex,
                        EndPage = -1,
                        EndIndex = -1
                    };
                    TextSelectionState = _startTextSelectionState;
                }

                _isSelectingText = true;
                sender.CaptureMouse();
            }
            else
            {
                _isSelectingText = false;
                _isSelectingWord = false;
                sender.ReleaseMouseCapture();
                _startTextSelectionState = null;
                TextSelectionState = null;
            }
            return true;
        }

        internal void HandleMouseUpForTextSelection(DjVuImage sender)
        {
            _isSelectingText = false;
            _isSelectingWord = false;
            _isDragAndDropText = false;
            sender.ReleaseMouseCapture();
            TextSelectionState?.Normalize();
            UpdateAdorner();
        }

        internal void HandleMouseMoveForTextSelection(DjVuImage sender, int page, Size viewSize, Point mouseLocation)
        {
            if (_isDragAndDropText)
            {
                // move existing selection -> Start drag and drop
                var text = SelectedText;
                DragDrop.DoDragDrop(sender, text, DragDropEffects.Copy);
                _isDragAndDropText = false;
                _isSelectingText = false;
                _isSelectingWord = false;
                return;
            }

            var mouseState = GetMouseState(page, viewSize, mouseLocation);
            var link = sender.PageLinks.GetLinkOnLocation(mouseState.DjVuLocation.Location);
            if (link != null)
            {
                Cursor = Cursors.Hand;
            }
            else if (mouseState.CharacterIndex >= 0)
            {
                Cursor = Cursors.IBeam;
                if (_isSelectingText)
                {
                    if (_isSelectingWord)
                    {
                        if (Document.GetWordAtPosition(mouseState.DjVuLocation, 4f, 4f, out var word))
                        {
                            var newSelectionWord = new DjVuTextSelectionState()
                            {
                                StartPage = mouseState.DjVuLocation.Page,
                                EndPage = mouseState.DjVuLocation.Page,
                                StartIndex = word.Offset,
                                EndIndex = word.Offset + word.Length
                            };
                            TextSelectionState = DjVuTextSelectionState.Merge(_startTextSelectionState, newSelectionWord);
                        }

                    }
                    else
                    {
                        var newSelectionLetter = new DjVuTextSelectionState()
                        {
                            StartPage = mouseState.DjVuLocation.Page,
                            EndPage = mouseState.DjVuLocation.Page,
                            StartIndex = mouseState.CharacterIndex,
                            EndIndex = mouseState.CharacterIndex
                        };
                        TextSelectionState = DjVuTextSelectionState.Merge(_startTextSelectionState, newSelectionLetter);
                    }
                    UpdateAdorner();
                }
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }

        internal bool HandleMouseDoubleClickForTextSelection(DjVuImage sender, int page, Size viewSize, Point mouseLocation)
        {
            var DjVuLocation = PointToDjVu(page, viewSize, mouseLocation);
            if (!DjVuLocation.IsValid)
                return false;

            if (Document.GetWordAtPosition(DjVuLocation, 4f, 4f, out var word))
            {
                _startTextSelectionState = new DjVuTextSelectionState()
                {
                    StartPage = DjVuLocation.Page,
                    EndPage = DjVuLocation.Page,
                    StartIndex = word.Offset,
                    EndIndex = word.Offset + word.Length
                };
                TextSelectionState = _startTextSelectionState;

                _isSelectingText = true;
                _isSelectingWord = true;
                sender.CaptureMouse();
                UpdateAdorner();
                return true;
            }

            return false;
        }

        private DjVuMouseState GetMouseState(int page, Size viewSize, Point mouseLocation)
        {
            // OnMouseMove and OnSetCursor get invoked a lot, often multiple times in succession for the same point.
            // By just caching the mouse state for the last known position we can save a lot of work.

            var currentState = _cachedMouseState;
            if (currentState?.DjVuLocation.Page == page && currentState?.MouseLocation == mouseLocation)
                return currentState;

            _cachedMouseState = new DjVuMouseState()
            {
                MouseLocation = mouseLocation,
                DjVuLocation = PointToDjVu(page, viewSize, mouseLocation)
            };

            if (!_cachedMouseState.DjVuLocation.IsValid)
                return _cachedMouseState;

            _cachedMouseState.CharacterIndex = Document.GetCharacterIndexAtPosition(_cachedMouseState.DjVuLocation, 4f, 4f);

            return _cachedMouseState;
        }

        /// <summary>
        /// Converts client coordinates to DjVu coordinates.
        /// </summary>
        /// <param name="location">Client coordinates to get the DjVu location for.</param>
        /// <returns>The location in a DjVu page or a DjVuPoint with IsValid false when the coordinates do not match a DjVu page.</returns>
        public DjVuPoint PointToDjVu(int page, Size viewSize, Point location)
        {
            if (Document == null)
                return DjVuPoint.Empty;

            var translated = TranslatePointToDjVu(viewSize, Document.PageSizes[page], location);
            translated = Document.PointToDjVu(page, new System.Drawing.Point((int)translated.X, (int)translated.Y));
            return new DjVuPoint(page, translated);
        }

        #region Links

        /// <summary>
        /// Occurs when a link in the DjVu document is clicked.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a link in the DjVu document is clicked.")]
        public event LinkClickEventHandler LinkClick;

        private void HandleLinkClick(LinkClickEventArgs e)
        {
            LinkClick?.Invoke(this, e);

            if (e.Handled)
                return;

            if (e.Link.TargetPage.HasValue)
            {
                GotoPage(e.Link.TargetPage.Value);
            }
            else if (!string.IsNullOrEmpty(e.Link.Uri))
            {
                try
                {
                    Process.Start(e.Link.Uri);
                }
                catch
                {
                    // Some browsers (Firefox) will cause an exception to
                    // be thrown (when it auto-updates).
                }
            }
        }

        internal void HandleMouseUpForLinks(DjVuImage sender, int page, Size viewSize, Point mouseLocation)
        {
            if (FollowLinkEnabled)
            {
                var mouseState = GetMouseState(page, viewSize, mouseLocation);
                var link = sender.PageLinks.GetLinkOnLocation(mouseState.DjVuLocation.Location);
                if (link != null)
                {
                    var linkClickEventArgs = new LinkClickEventArgs(link);
                    HandleLinkClick(linkClickEventArgs);
                }
            }
        }

        #endregion
    }
}
