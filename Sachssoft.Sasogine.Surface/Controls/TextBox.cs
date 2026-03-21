using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Services;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class TextBox : Widget
    {
        private StyleProperty<TextBoxMode> _mode = new StyleProperty<TextBoxMode>(TextBoxMode.Regular, isUserSet: false);
        private StyleProperty<int> _verticalSpacing = new StyleProperty<int>(0, isUserSet: false);
        private StyleProperty<string?> _text = new StyleProperty<string?>(null, isUserSet: false);
        private StyleProperty<string?> _hintText = new StyleProperty<string?>(null, isUserSet: false);
        private StyleProperty<char> _passwordChar = new StyleProperty<char>('•', isUserSet: false);
        private StyleProperty<bool> _wrap = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<Font> _font = new StyleProperty<Font>(DefaultStyle.TextFont, isUserSet: false);
        private StyleProperty<Color> _focusedTextColor = new StyleProperty<Color>(Color.White, isUserSet: false);
        private StyleProperty<Color> _selectionColor = new StyleProperty<Color>(Color.LightBlue, isUserSet: false);
        private StyleProperty<Color> _caretColor = new StyleProperty<Color>(Color.White, isUserSet: false);
        private StyleProperty<TimeSpan> _caretBlinkInterval = new StyleProperty<TimeSpan>(TimeSpan.FromMilliseconds(500), isUserSet: false);
        private StyleProperty<bool> _isReadOnly = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<VerticalAlignment> _textVerticalAlignment = new StyleProperty<VerticalAlignment>(Visuals.VerticalAlignment.Top, isUserSet: false);
        private StyleProperty<TextBoxOverflowMode> _overflowMode = new StyleProperty<TextBoxOverflowMode>(TextBoxOverflowMode.AutoWidth, isUserSet: false);

        private const int CaretUpdateDelayInMilliseconds = 30;
        private const int CaretWidth = 1;
        private int _caretIndex;
        private SolidColorBrush _selectionBrush;
        private SolidColorBrush _caretBrush;
        private DateTime _lastCaretUpdate;
        private DateTime _lastCaretBlinkStamp = DateTime.Now;
        private bool _caretDisplayed = true;
        private readonly RichTextLayout _richTextLayout = new RichTextLayout
        {
            CalculateGlyphs = true,
            SupportsCommands = false
        };
        private Point? _lastCursorPosition;
        private Point _internalScrolling = Point.Zero;
        private bool _suppressRedoStackReset = false;
        private bool _isTouchDown;

        private readonly UndoRedoStack UndoStack = new UndoRedoStack();
        private readonly UndoRedoStack RedoStack = new UndoRedoStack();

        #region Events

        /// <summary>
        /// Fires when the value is about to be changed
        /// Set Cancel to true if you want to cancel the change
        /// </summary>
        public event EventHandler<ValueChangingEventArgs<string?>>? ValueChanging;

        /// <summary>
        /// Fires every time when the text had been changed
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<string?>>? TextChanged;

        /// <summary>
        /// Fires every time when the text had been changed by user(doesnt fire if it had been assigned through code)
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<string?>>? TextChangedByUser;

        /// <summary>
        /// Fires every time when the text had been deleted
        /// </summary>
        public event EventHandler<TextDeletedEventArgs>? TextDeleted;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler? CursorPositionChanged;

        #endregion

        public TextBox()
        {
            AcceptsKeyboardFocus = AcceptsKeyboardFocus.Override(true);
            MouseCursor = MouseCursor.Override(MouseCursorType.IBeam);
            ClipToBounds = ClipToBounds.Override(true);
            HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Stretch);
            VerticalAlignment = VerticalAlignment.Override(Visuals.VerticalAlignment.Top);

            UpdateCaretColor();
            UpdateSelectionColor();
            UpdateRichTextLayout();
        }

        #region Style Properties

        public StyleProperty<int> VerticalSpacing
        {
            get => _verticalSpacing;
            set
            {
                if (SetAndNotify(ref _verticalSpacing, value))
                {
                    UpdateRichTextLayout();
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<string?> Text
        {
            get => _text;
            set
            {
                if (SetAndNotify(ref _text, value))
                {
                    SetText(value, false);
                    DisableHintText();
                }
            }
        }

        public StyleProperty<string?> HintText
        {
            get => _hintText;
            set
            {
                if (SetAndNotify(ref _hintText, value))
                {
                    EnableHintText();
                }
            }
        }

        //[Obsolete("TextboxMode!")]
        //public StyleProperty<bool> IsMultiline
        //{
        //    get => _isMultiline;
        //    set => SetAndNotify(ref _isMultiline, value);
        //}

        public StyleProperty<Font> Font
        {
            get => _font;
            set
            {
                if (SetAndNotify(ref _font, value))
                {
                    UpdateRichTextLayout();
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<bool> Wrap
        {
            get => _wrap;
            set
            {
                if (SetAndNotify(ref _wrap, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<TextBoxOverflowMode> OverflowMode
        {
            get => _overflowMode;
            set
            {
                if (SetAndNotify(ref _overflowMode, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<Color> FocusedTextColor
        {
            get => _focusedTextColor;
            set => SetAndNotify(ref _focusedTextColor, value);
        }

        public StyleProperty<Color> CaretColor
        {
            get => _caretColor;
            set
            {
                if (SetAndNotify(ref _caretColor, value))
                {
                    UpdateCaretColor();
                }
            }
        }

        public StyleProperty<Color> SelectionColor
        {
            get => _selectionColor;
            set
            {
                if (SetAndNotify(ref _selectionColor, value))
                {
                    UpdateSelectionColor();
                }
            }
        }

        public StyleProperty<TimeSpan> CaretBlinkInterval
        {
            get => _caretBlinkInterval;
            set => SetAndNotify(ref _caretBlinkInterval, value);
        }

        public StyleProperty<bool> IsReadOnly
        {
            get => _isReadOnly;
            set => SetAndNotify(ref _isReadOnly, value);
        }

        //[Obsolete("TextboxMode!")]
        //public StyleProperty<bool> PasswordField
        //{
        //    get => _passwordField;
        //    set
        //    {
        //        _passwordField = value;
        //        UpdateRichTextLayout();
        //    }
        //}

        public StyleProperty<char> PasswordChar
        {
            get => _passwordChar;
            set
            {
                if (SetAndNotify(ref _passwordChar, value))
                {
                    UpdateRichTextLayout();
                }
            }
        }

        public StyleProperty<TextBoxMode> Mode
        {
            get => _mode;
            set
            {
                if (SetAndNotify(ref _mode, value))
                {
                    UpdateRichTextLayout();
                }
            }
        }

        #endregion

        #region Direct Properties

        public bool IsMultiline => _mode.Value == TextBoxMode.Multiline ||
                                   _mode.Value == TextBoxMode.RichText;

        public int CursorPosition
        {
            get => _caretIndex;
            set
            {
                if (SetAndNotify(ref _caretIndex, value))
                {
                    OnCursorIndexChanged();
                }
            }
        }

        public int SelectionStartIndex { get; private set; }

        public int SelectionEndIndex { get; private set; }

        public bool IsHintTextEnabled { get; private set; }

        //private int CursorWidth => 1;   //1 + (CaretColor.Value != null ? CaretColor.Value.Size.X : 0);

        [AllowNull]
        public override Desktop Desktop
        {
            get => base.Desktop;
            internal set
            {
                if (Desktop != null)
                {
                    Desktop.TouchUp -= DesktopTouchUp;
                    Desktop.TouchDown -= DesktopTouchDown;
                }

                base.Desktop = value;

                if (Desktop != null)
                {
                    Desktop.TouchUp += DesktopTouchUp;
                    Desktop.TouchDown += DesktopTouchDown;
                }
            }
        }

        #endregion

        private string? UserText
        {
            get
            {
                return _text;
            }

            set
            {
                SetText(value, true);
            }
        }

        public int TextLength => _text.Value?.Length ?? 0;

        private bool InsertMode { get; set; }

        /// <summary>
        /// Cursor position in local coordinates
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public Point CursorCoords => GetRenderPositionByIndex(CursorPosition);

        private void DeleteChars(int pos, int l)
        {
            if (l == 0 || string.IsNullOrEmpty(UserText))
                return;

            UserText = UserText.Substring(0, pos) + UserText.Substring(pos + l);
        }

        private bool InsertChars(int pos, string? s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            if (string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(UserText))
            {
                UserText = s;
            }
            else
            {
                UserText = UserText.Substring(0, pos) + s + UserText.Substring(pos);
            }

            return true;
        }

        private bool InsertChar(int pos, char ch)
        {
            if (string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(UserText))
            {
                UserText = ch.ToString();
            }
            else
            {
                UserText = UserText.Substring(0, pos) + ch + UserText.Substring(pos);
            }

            return true;
        }

        public void Insert(int where, string? text)
        {
            text = Process(text);
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (InsertChars(where, text))
            {
                UndoStack.MakeInsert(where, text.Length);
                CursorPosition += text.Length;
            }
        }

        public void Replace(int where, int len, string? text)
        {
            if (len <= 0)
            {
                Insert(where, text);
                return;
            }

            text = Process(text);

            if (string.IsNullOrEmpty(text))
            {
                Delete(where, len);
                return;
            }

            UndoStack.MakeReplace(Text, where, len, text.Length);

            if (!string.IsNullOrEmpty(UserText))
                UserText = UserText.Substring(0, where) + text + UserText.Substring(where + len);
        }

        public void ReplaceAll(string text)
        {
            if (string.IsNullOrEmpty(Text))
            {
                Replace(0, 0, text);
            }
            else
            {
                Replace(0, TextLength, text);
            }
        }

        private int Delete(int where, int len)
        {
            if (where < 0 || where >= TextLength || len < 0)
            {
                return 0;
            }

            // If we're trying to delete one part
            // of a surrogate pair, delete both.
            if (len == 1)
            {
                if (char.IsSurrogate(GetTextChar(where)))
                {
                    len++;
                }

                if (char.IsLowSurrogate(GetTextChar(where)))
                {
                    where--;
                }
            }

            UndoStack.MakeDelete(Text, where, len);
            var stringToDelete = EnsureGetText().Substring(where, len);
            DeleteChars(where, len);

            TextDeleted?.Invoke(this, new TextDeletedEventArgs(where, stringToDelete));
            return len;
        }

        private void DeleteSelection()
        {
            if (SelectionStartIndex != SelectionEndIndex)
            {
                if (SelectionStartIndex < SelectionEndIndex)
                {
                    Delete(SelectionStartIndex, SelectionEndIndex - SelectionStartIndex);
                    SelectionEndIndex = CursorPosition = SelectionStartIndex;
                }
                else
                {
                    Delete(SelectionEndIndex, SelectionStartIndex - SelectionEndIndex);
                    SelectionStartIndex = CursorPosition = SelectionEndIndex;
                }
            }
        }

        private bool Paste(string? text)
        {
            text = Process(text);

            DeleteSelection();
            if (InsertChars(CursorPosition, text))
            {
                UndoStack.MakeInsert(CursorPosition, TextLength);
                CursorPosition += TextLength;
                ResetSelection();
                return true;
            }

            return false;
        }

        private void InputChar(char ch)
        {
            if (!IsMultiline && ch == '\n')
                return;

            if (InsertMode && !(SelectionStartIndex != SelectionEndIndex) && CursorPosition < TextLength)
            {
                UndoStack.MakeReplace(Text, CursorPosition, 1, 1);
                DeleteChars(CursorPosition, 1);
                if (InsertChar(CursorPosition, ch))
                {
                    UserSetCursorPosition(CursorPosition + 1);

                }
            }
            else
            {
                DeleteSelection();
                if (InsertChar(CursorPosition, ch))
                {
                    UndoStack.MakeInsert(CursorPosition, 1);
                    UserSetCursorPosition(CursorPosition + 1);
                }
            }

            ResetSelection();
        }

        private void UndoRedo(UndoRedoStack undoStack, UndoRedoStack redoStack)
        {
            if (undoStack.Stack.Count == 0)
            {
                return;
            }

            var record = undoStack.Stack.Pop();
            try
            {
                _suppressRedoStackReset = true;
                switch (record.OperationType)
                {
                    case OperationType.Insert:
                        redoStack.MakeDelete(Text, record.Where, record.Length);
                        DeleteChars(record.Where, record.Length);
                        UserSetCursorPosition(record.Where);
                        break;
                    case OperationType.Delete:
                        if (InsertChars(record.Where, record.Data))
                        {
                            redoStack.MakeInsert(record.Where, record.Data.Length);
                            UserSetCursorPosition(record.Where + record.Data.Length);
                        }
                        break;
                    case OperationType.Replace:
                        redoStack.MakeReplace(Text, record.Where, record.Length, record.Data.Length);
                        DeleteChars(record.Where, record.Length);
                        InsertChars(record.Where, record.Data);
                        break;
                }
            }
            finally
            {
                _suppressRedoStackReset = false;
            }

            ResetSelection();
        }

        private void Undo()
        {
            UndoRedo(UndoStack, RedoStack);
        }

        private void Redo()
        {
            UndoRedo(RedoStack, UndoStack);
        }

        private void UserSetCursorPosition(int newPosition)
        {
            if (newPosition > TextLength)
            {
                newPosition = TextLength;
            }

            if (newPosition < 0)
            {
                newPosition = 0;
            }

            CursorPosition = newPosition;
        }

        private void ResetSelection()
        {
            SelectionStartIndex = SelectionEndIndex = CursorPosition;
        }

        private void UpdateSelection()
        {
            SelectionEndIndex = CursorPosition;
        }

        private void UpdateSelectionIfShiftDown()
        {
            if (Desktop.KeyboardManager.IsShiftDown)
            {
                UpdateSelection();
            }
            else
            {
                ResetSelection();
            }
        }

        private void MoveLine(int delta)
        {
            var line = _richTextLayout.GetLineByCursorPosition(CursorPosition);
            if (line == null)
            {
                return;
            }

            var newLine = line.LineIndex + delta;
            if (newLine < 0 || newLine >= _richTextLayout.Lines.Count)
            {
                return;
            }

            var bounds = ActualBounds;
            var pos = GetRenderPositionByIndex(CursorPosition);
            var preferredX = pos.X - bounds.X;

            // Find closest glyph
            var newString = _richTextLayout.Lines[newLine];
            var cursorPosition = newString.TextStartIndex;
            var glyphIndex = newString.GetGlyphIndexByX(preferredX);
            if (glyphIndex != null)
            {
                cursorPosition += glyphIndex.Value;
            }

            UserSetCursorPosition(cursorPosition);
            UpdateSelectionIfShiftDown();
        }

        public void SelectAll()
        {
            // Select all
            SelectionStartIndex = 0;
            SelectionEndIndex = TextLength;
        }

        public override void OnKeyDown(Keys k)
        {
            base.OnKeyDown(k);

            if (!IsEnabled)
                return;

            switch (k)
            {
                case Keys.C:
                    if (Desktop.KeyboardManager.IsControlDown)
                    {
                        Copy();
                    }
                    break;

                case Keys.V:
                    if (!IsReadOnly && Desktop.KeyboardManager.IsControlDown)
                    {
                        var clipboardText = ClipboardServiceHelper.GetText();

                        if (!string.IsNullOrEmpty(clipboardText))
                        {
                            Paste(clipboardText);
                        }
                    }
                    break;

                case Keys.X:
                    if (Desktop.KeyboardManager.IsControlDown)
                    {
                        Copy();
                        if (!IsReadOnly && SelectionStartIndex != SelectionEndIndex)
                        {
                            DeleteSelection();
                        }
                    }
                    break;

                case Keys.D:
                    if (!IsReadOnly && Desktop.KeyboardManager.IsControlDown)
                    {
                        // nothing selected -> duplicate current line
                        if (SelectionStartIndex == SelectionEndIndex)
                        {
                            var ensureText = EnsureGetText();

                            // get start of line
                            var searchStart = Math.Max(0, SelectionStartIndex - 1);
                            var lineStart = ensureText.LastIndexOf("\n", searchStart);
                            // special case: cursor is in first line
                            if (lineStart == -1) lineStart = 0;

                            // get end of line
                            var lineEnd = ensureText.IndexOf("\n", SelectionEndIndex);
                            // special case: cursor is in last line
                            if (lineEnd == -1) lineEnd = TextLength;

                            var line = ensureText.Substring(lineStart, lineEnd - lineStart);
                            if (lineStart == 0)
                                line = "\n" + line;
                            Insert(lineEnd, line);
                        }
                        // duplicate selection
                        else
                        {
                            var start = Math.Min(SelectionStartIndex, SelectionEndIndex);
                            var end = Math.Max(SelectionStartIndex, SelectionEndIndex);
                            var text = EnsureGetText().Substring(start, end - start);
                            Insert(end, text);
                        }
                    }
                    break;

                case Keys.Insert:
                    if (!IsReadOnly)
                    {
                        InsertMode = !InsertMode;
                    }
                    break;

                case Keys.Z:
                    if (!IsReadOnly && Desktop.KeyboardManager.IsControlDown)
                    {
                        Undo();
                    }
                    break;

                case Keys.Y:
                    if (!IsReadOnly && Desktop.KeyboardManager.IsControlDown)
                    {
                        Redo();
                    }
                    break;

                case Keys.A:
                    if (Desktop.KeyboardManager.IsControlDown)
                    {
                        SelectAll();
                    }
                    break;

                case Keys.Left:
                    if (CursorPosition > 0)
                    {
                        UserSetCursorPosition(CursorPosition - 1);
                        UpdateSelectionIfShiftDown();
                    }
                    break;

                case Keys.Right:
                    if (CursorPosition < TextLength)
                    {
                        UserSetCursorPosition(CursorPosition + 1);
                        UpdateSelectionIfShiftDown();
                    }
                    break;

                case Keys.Up:
                    MoveLine(-1);
                    break;

                case Keys.Down:
                    MoveLine(1);
                    break;

                case Keys.Back:
                    if (!IsReadOnly)
                    {
                        if (SelectionStartIndex == SelectionEndIndex)
                        {
                            int deleted = Delete(CursorPosition - 1, 1);
                            if (deleted > 0)
                            {
                                UserSetCursorPosition(CursorPosition - deleted);
                                ResetSelection();
                            }
                        }
                        else
                        {
                            DeleteSelection();
                        }
                    }
                    break;

                case Keys.Delete:
                    if (!IsReadOnly)
                    {
                        if (SelectionStartIndex == SelectionEndIndex)
                        {
                            Delete(CursorPosition, 1);
                        }
                        else
                        {
                            DeleteSelection();
                        }
                    }
                    break;

                case Keys.Home:
                    {
                        if (!Desktop.KeyboardManager.IsControlDown && !string.IsNullOrEmpty(Text))
                        {
                            var newPosition = CursorPosition;
                            var ensureText = EnsureGetText();

                            while (newPosition > 0 && (newPosition - 1 >= TextLength || ensureText[newPosition - 1] != '\n'))
                            {
                                --newPosition;
                            }

                            UserSetCursorPosition(newPosition);
                        }
                        else
                        {
                            UserSetCursorPosition(0);
                        }

                        UpdateSelectionIfShiftDown();

                        break;
                    }

                case Keys.End:
                    {
                        if (!Desktop.KeyboardManager.IsControlDown)
                        {
                            var newPosition = CursorPosition;
                            var ensureText = EnsureGetText();

                            while (newPosition < TextLength && ensureText[newPosition] != '\n')
                            {
                                ++newPosition;
                            }

                            UserSetCursorPosition(newPosition);
                        }
                        else
                        {
                            UserSetCursorPosition(TextLength);
                        }

                        UpdateSelectionIfShiftDown();

                        break;
                    }

                case Keys.Enter:
                    if (!IsReadOnly)
                    {
                        InputChar('\n');
                    }
                    break;
            }
        }

        private void Copy()
        {
            if (SelectionEndIndex != SelectionStartIndex)
            {
                var selectStart = Math.Min(SelectionStartIndex, SelectionEndIndex);
                var selectEnd = Math.Max(SelectionStartIndex, SelectionEndIndex);

                var clipboardText = _richTextLayout.Text.Substring(selectStart, selectEnd - selectStart);
                ClipboardServiceHelper.SetText(clipboardText);
            }
        }

        private static string? Process(string? value)
        {
            // Remove '\r'
            if (value != null)
            {
                value = value.Replace("\r", string.Empty);
            }

            return value;
        }

        private bool SetText(string? value, bool byUser)
        {
            value = Process(value);
            if (value == _text)
            {
                return false;
            }

            var oldValue = _text;
            if (ValueChanging != null)
            {
                var args = new ValueChangingEventArgs<string?>(oldValue, value);
                ValueChanging(this, args);
                if (args.Cancel)
                {
                    return false;
                }

                value = args.NewValue;
            }

            _text = value;

            UpdateRichTextLayout();

            if (!byUser)
            {
                CursorPosition = SelectionStartIndex = SelectionEndIndex = 0;
            }

            if (!_suppressRedoStackReset)
            {
                RedoStack.Reset();
            }

            InvalidateMeasure();

            TextChanged?.Invoke(this, new ValueChangedEventArgs<string?>(oldValue, value));

            if (byUser)
            {
                TextChangedByUser?.Invoke(this, new ValueChangedEventArgs<string?>(oldValue, value));
            }

            return true;
        }

        private void UpdateRichTextLayout()
        {
            _richTextLayout.VerticalSpacing = _verticalSpacing;
            _richTextLayout.Font = _font.Value.GetSpriteFont(Stylesheet.Current);

            if (string.IsNullOrEmpty(_text))
            {
                _richTextLayout.Text = _text;
                EnableHintText();
                return;
            }

            DisableHintText();
            _richTextLayout.Text = _mode.Value == TextBoxMode.Password ?
                new string(_passwordChar.Value, TextLength) :
                _text;
        }

        private void DisableHintText()
        {
            if (_hintText == null)
            {
                return;
            }

            _richTextLayout.Text = _text;
            IsHintTextEnabled = false;
        }

        private void EnableHintText()
        {
            if (ShouldEnableHintText())
            {
                _richTextLayout.Text = _hintText;
                IsHintTextEnabled = true;
            }
        }

        private bool ShouldEnableHintText()
        {
            return _hintText != null &&
                   string.IsNullOrEmpty(_text)
                   && !IsKeyboardFocused;
        }

        private void UpdateScrolling()
        {
            var p = GetRenderPositionByIndex(CursorPosition);
            if (p == _lastCursorPosition || Desktop == null)
            {
                return;
            }

            Desktop.UpdateLayout();

            var asScrollViewer = Parent as ScrollViewer;

            Point sz, maximum;
            var bounds = ActualBounds;
            if (asScrollViewer != null)
            {
                sz = new Point(asScrollViewer.Bounds.Width, asScrollViewer.Bounds.Height);
                sz.X -= asScrollViewer.VerticalThumbWidth;
                sz.Y -= asScrollViewer.HorizontalThumbHeight;

                maximum = asScrollViewer.ScrollMaximum;
            }
            else
            {
                sz = new Point(Bounds.Width, Bounds.Height);
                maximum = new Point(_richTextLayout.Size.X + CaretWidth - sz.X,
                    _richTextLayout.Size.Y - sz.Y);

                if (maximum.X < 0)
                {
                    maximum.X = 0;
                }

                if (maximum.Y < 0)
                {
                    maximum.Y = 0;
                }
            }

            if (maximum == Point.Zero)
            {
                _internalScrolling = Point.Zero;
                _lastCursorPosition = p;
                return;
            }

            p.X -= bounds.X;
            p.Y -= bounds.Y;

            var lineHeight = _richTextLayout.Font.LineHeight;

            Point sp;
            if (asScrollViewer != null)
            {
                sp = asScrollViewer.ScrollPosition;
            }
            else
            {
                sp = _internalScrolling;
            }

            if (p.Y < sp.Y)
            {
                sp.Y = p.Y;
            }
            else if (p.Y + lineHeight > sp.Y + sz.Y)
            {
                sp.Y = p.Y + lineHeight - sz.Y;
            }

            if (p.X < sp.X)
            {
                sp.X = p.X;
            }
            else if (p.X + CaretWidth > sp.X + sz.X)
            {
                sp.X = p.X + CaretWidth - sz.X;
            }

            if (asScrollViewer != null)
            {
                if (sp.X < 0)
                {
                    sp.X = 0;
                }

                if (sp.X > maximum.X)
                {
                    sp.X = maximum.X;
                }

                if (sp.Y < 0)
                {
                    sp.Y = 0;
                }

                if (sp.Y > maximum.Y)
                {
                    sp.Y = maximum.Y;
                }

                asScrollViewer.ScrollPosition = sp;
            }
            else
            {
                if (sp.X < 0)
                {
                    sp.X = 0;
                }

                if (sp.X > maximum.X)
                {
                    sp.X = maximum.X;
                }

                if (sp.Y < 0)
                {
                    sp.Y = 0;
                }

                if (sp.Y > maximum.Y)
                {
                    sp.Y = maximum.Y;
                }

                _internalScrolling = sp;
            }

            _lastCursorPosition = p;
        }

        private void OnCursorIndexChanged()
        {
            _lastCaretUpdate = DateTime.Now;

            UpdateScrolling();

            CursorPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void OnChar(char c)
        {
            base.OnChar(c);

            if (!IsEnabled)
                return;

            if (!IsReadOnly && !char.IsControl(c))
            {
                InputChar(c);
            }
        }

        private char GetTextChar(int index)
        {
            if (string.IsNullOrEmpty(Text.Value))
                return default;

            return Text.Value[index];
        }

        private string EnsureGetText()
        {
            if (string.IsNullOrEmpty(Text.Value))
                return string.Empty;

            return Text.Value;
        }

        private void SetCursorByTouch()
        {
            if (Desktop == null)
            {
                return;
            }

            var mousePos = ToLocal(Desktop.TouchPosition.GetValueOrDefault());
            mousePos.X += _internalScrolling.X;
            mousePos.Y += _internalScrolling.Y;

            if (mousePos.X < 0)
            {
                mousePos.X = 0;
            }

            if (mousePos.Y < 0)
            {
                mousePos.Y = 0;
            }

            var line = _richTextLayout.GetLineByY(mousePos.Y);
            if (line != null)
            {
                var glyphIndex = line.GetGlyphIndexByX(mousePos.X);
                if (glyphIndex != null)
                {
                    UserSetCursorPosition(line.TextStartIndex + glyphIndex.Value);
                    if (_isTouchDown || Desktop.KeyboardManager.IsShiftDown)
                    {
                        UpdateSelection();
                    }
                    else
                    {
                        ResetSelection();
                    }
                }
            }
        }

        private void DesktopTouchUp(object? sender, EventArgs args)
        {
            _isTouchDown = false;
        }

        private void DesktopTouchDown(object? sender, EventArgs e)
        {
            if (!IsEnabled || !IsTouchInside || TextLength == 0)
            {
                return;
            }

            SetCursorByTouch();

            _lastCaretUpdate = DateTime.Now;
            _isTouchDown = true;
        }


        internal protected override void OnTouchDoubleClick()
        {
            base.OnTouchDoubleClick();

            var position = CursorPosition;
            var ensureText = EnsureGetText();

            if (string.IsNullOrEmpty(ensureText) || position < 0 || position >= TextLength || Desktop.KeyboardManager.IsShiftDown)
            {
                return;
            }

            if (char.IsWhiteSpace(ensureText[position]))
            {
                if (position == 0)
                {
                    return;
                }

                --position;
                if (char.IsWhiteSpace(ensureText[position]))
                {
                    return;
                }
            }

            int start, end;
            start = end = position;

            while (start > 0)
            {
                if (char.IsWhiteSpace(ensureText[start]))
                {
                    ++start;
                    break;
                }

                --start;
            }

            while (end < TextLength)
            {
                if (char.IsWhiteSpace(ensureText[end]))
                {
                    break;
                }

                ++end;
            }

            if (start == end)
            {
                return;
            }

            SelectionStartIndex = start;
            SelectionEndIndex = end;
        }

        public override void OnKeyboardFocusGot()
        {
            base.OnKeyboardFocusGot();

            _lastCaretBlinkStamp = DateTime.Now;
            _caretDisplayed = true;

            DisableHintText();
        }

        public override void OnKeyboardFocusLost()
        {
            base.OnKeyboardFocusLost();

            EnableHintText();
        }

        private Point GetRenderPositionByIndex(int index)
        {
            var bounds = ActualBounds;

            var x = bounds.X;
            var y = bounds.Y;

            if (Text != null)
            {
                if (index < TextLength)
                {
                    var glyphRender = _richTextLayout.GetGlyphInfoByIndex(index);
                    if (glyphRender != null)
                    {
                        x += glyphRender.Value.Bounds.Left;
                        y += glyphRender.Value.LineTop;
                    }
                }
                else if (_richTextLayout.Lines != null && _richTextLayout.Lines.Count > 0)
                {
                    // After last glyph
                    var lastLine = _richTextLayout.Lines[_richTextLayout.Lines.Count - 1];
                    if (lastLine.Count > 0)
                    {
                        var glyphRender = lastLine.GetGlyphInfoByIndex(lastLine.Count - 1).GetValueOrDefault();

                        x += glyphRender.Bounds.Left + glyphRender.XAdvance;
                        y += glyphRender.LineTop;
                    }
                    else if (_richTextLayout.Lines.Count > 1)
                    {
                        var previousLine = _richTextLayout.Lines[_richTextLayout.Lines.Count - 2];
                        if (previousLine.Count > 0)
                        {
                            var glyphRender = previousLine.GetGlyphInfoByIndex(0).GetValueOrDefault();
                            y += glyphRender.LineTop + lastLine.Size.Y + _richTextLayout.VerticalSpacing;
                        }
                    }
                }
            }

            return new Point(x, y);
        }

        [MemberNotNull(nameof(_caretBrush))]
        private void UpdateCaretColor()
        {
            _caretBrush = new SolidColorBrush(_caretColor);
        }

        [MemberNotNull(nameof(_selectionBrush))]
        private void UpdateSelectionColor()
        {
            _selectionBrush = new SolidColorBrush(_selectionColor);
        }

        private void RenderSelection(RenderContext context)
        {
            var bounds = ActualBounds;

            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            var selectStart = Math.Min(SelectionStartIndex, SelectionEndIndex);
            var selectEnd = Math.Max(SelectionStartIndex, SelectionEndIndex);

            if (selectStart >= selectEnd)
            {
                return;
            }

            var startGlyph = _richTextLayout.GetGlyphInfoByIndex(selectStart);
            if (startGlyph == null)
            {
                return;
            }

            var lineIndex = startGlyph.Value.TextChunk.LineIndex;
            var i = selectStart;

            var lineHeight = _richTextLayout.Font.LineHeight;
            while (true)
            {
                startGlyph = _richTextLayout.GetGlyphInfoByIndex(i);
                if (startGlyph == null)
                {
                    break;
                }

                var startPosition = GetRenderPositionByIndex(i);

                var line = _richTextLayout.Lines[startGlyph.Value.TextChunk.LineIndex];

                if (selectEnd < line.TextStartIndex + line.Count)
                {
                    var endPosition = GetRenderPositionByIndex(selectEnd);

                    _selectionBrush.Draw(context,
                        new Rectangle(startPosition.X - _internalScrolling.X,
                            startPosition.Y - _internalScrolling.Y,
                            endPosition.X - startPosition.X,
                            lineHeight));

                    break;
                }

                _selectionBrush.Draw(context,
                    new Rectangle(startPosition.X - _internalScrolling.X,
                        startPosition.Y - _internalScrolling.Y,
                        bounds.Left + startGlyph.Value.TextChunk.Size.X - startPosition.X,
                        lineHeight));

                ++lineIndex;
                if (lineIndex >= _richTextLayout.Lines.Count)
                {
                    break;
                }

                i = _richTextLayout.Lines[lineIndex].TextStartIndex;
            }
        }

        public override void InternalRender(RenderContext context, GameTime time)
        {
            if (_richTextLayout.Font == null)
            {
                return;
            }

            if (_isTouchDown)
            {
                // This makes the text to scroll if the touch is outside of the TextBox bounds
                var passed = DateTime.Now - _lastCaretUpdate;
                if (passed.TotalMilliseconds > CaretUpdateDelayInMilliseconds)
                {
                    SetCursorByTouch();
                    _lastCaretUpdate = DateTime.Now;
                }
            }

            var bounds = ActualBounds;
            RenderSelection(context);

            var textColor = TextColor;
            var oldOpacity = context.Opacity;

            if (IsHintTextEnabled)
            {
                context.Opacity *= 0.5f;
            }
            else if (!IsEnabled)
            {
                textColor = DisabledTextColor.Value;
            }
            else if (IsKeyboardFocused)
            {
                textColor = FocusedTextColor.Value;
            }

            var centeredBounds = LayoutUtils.Align(
                new Point(bounds.Width, bounds.Height),
                _richTextLayout.Size,
                Visuals.HorizontalAlignment.Left,
                TextVerticalAlignment.Value
            );

            centeredBounds.Offset(bounds.Location);

            var p = new Point(centeredBounds.Location.X - _internalScrolling.X,
                centeredBounds.Location.Y - _internalScrolling.Y);

            if (UIEnvironment.DrawTextGlyphsFrames)
            {
                foreach (var line in _richTextLayout.Lines)
                {
                    foreach (TextChunk chunk in line.Chunks)
                    {
                        foreach (var glyph in chunk.Glyphs)
                        {
                            var glyphBounds = glyph.Bounds;
                            glyphBounds.Offset(p);
                            context.DrawRectangle(glyphBounds, Color.White);
                        }
                    }
                }
            }

            context.DrawRichText(_richTextLayout, new Vector2(p.X, p.Y), textColor);

            if (!IsKeyboardFocused)
            {
                // Skip cursor rendering if the widget doesnt have the focus
                return;
            }

            var now = DateTime.Now;

            if (_lastCaretUpdate > _lastCaretBlinkStamp)
            {
                _lastCaretBlinkStamp = _lastCaretUpdate;
                _caretDisplayed = true;
            }

            if ((now - _lastCaretBlinkStamp).TotalMilliseconds >= _caretBlinkInterval.Value.TotalMilliseconds)
            {
                _caretDisplayed = !_caretDisplayed;
                _lastCaretBlinkStamp = now;
            }

            if (IsEnabled && _caretDisplayed && CaretColor.Value != null)
            {
                p = GetRenderPositionByIndex(CursorPosition);

                p.X -= _internalScrolling.X;
                p.Y -= _internalScrolling.Y;

                var rect = new Rectangle(p.X, p.Y, CaretWidth, _richTextLayout.Font.LineHeight);
                _caretBrush.Draw(context, rect, Color.White);
            }

            context.Opacity = oldOpacity;
        }

        protected override Point InternalMeasure(Point availableSize)
        {
            if (Font.Value == null)
            {
                return Point.Zero;
            }

            var width = availableSize.X;
            width -= CaretWidth;

            var result = _richTextLayout.Measure(_wrap ? width : default(int?));
            var fontType = Font.Value.GetSpriteFont(Stylesheet.Current);

            if (result.Y < fontType.LineHeight)
            {
                result.Y = fontType.LineHeight;
            }

            // +++++++ Test - Verhindern von automatischen Breite bei Überlänge +++++++
            //
            if (_overflowMode.Value == TextBoxOverflowMode.Clip &&
                (!MaxWidth.Value.HasValue || !Width.Value.HasValue))
            {
                result.X = 0;
            }
            //

            if (CaretColor.Value != null)
            {
                result.X += CaretWidth;
                result.Y = Math.Max(result.Y, CaretWidth /*CaretColor.Value.Size.Y*/);
            }

            return result;
        }

        protected override void InternalArrange()
        {
            base.InternalArrange();

            var width = ActualBounds.Width;
            width -= CaretWidth;

            _richTextLayout.Width = _wrap ? width : default(int?);
        }

        public float GetWidth(int index)
        {
            var glyph = _richTextLayout.GetGlyphInfoByIndex(index);
            if (glyph == null)
            {
                return 0;
            }

            if (glyph.Value.Codepoint == '\n')
            {
                return 0;
            }

            return glyph.Value.Bounds.Width;
        }

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(Background):
                        target.Background = target.Background.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(TextColor):
                        target.TextColor = target.TextColor.Override(value.ConvertTo<Color>());
                        break;
                    case nameof(DisabledTextColor):
                        target.DisabledTextColor = target.DisabledTextColor.Override(value.ConvertTo<Color>());
                        break;
                    case nameof(Font):
                        target.Font = target.Font.Override(sheet.GetFont(value.RawValue));
                        break;
                    case nameof(CaretColor):
                        target.CaretColor = target.CaretColor.Override(value.ConvertTo<Color>());
                        break;
                    case nameof(SelectionColor):
                        target.SelectionColor = target.SelectionColor.Override(value.ConvertTo<Color>());
                        break;

                }
            });
        }
        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not TextBox textBox)
                return;

            VerticalSpacing = textBox.VerticalSpacing;
            Text = textBox.Text;
            HintText = textBox.HintText;
            IsHintTextEnabled = textBox.IsHintTextEnabled;

            Font = textBox.Font;
            Wrap = textBox.Wrap;
            OverflowMode = textBox.OverflowMode;

            TextColor = textBox.TextColor;
            DisabledTextColor = textBox.DisabledTextColor;
            FocusedTextColor = textBox.FocusedTextColor;
            CaretColor = textBox.CaretColor;
            SelectionColor = textBox.SelectionColor;

            CaretBlinkInterval = textBox.CaretBlinkInterval;
            IsReadOnly = textBox.IsReadOnly;

            PasswordChar = textBox.PasswordChar;
            Mode = textBox.Mode;
            TextVerticalAlignment = textBox.TextVerticalAlignment;

            // Optional: Cursor & Selection
            CursorPosition = textBox.CursorPosition;
            SelectionStartIndex = textBox.SelectionStartIndex;
            SelectionEndIndex = textBox.SelectionEndIndex;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new TextBox();
        }

        #endregion
    }
}