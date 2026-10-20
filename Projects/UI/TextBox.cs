using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Engine.Controls;
/// <summary>Editable Unicode text with selection, clipboard, undo/redo and optional multiple lines.</summary>
public class TextBox : Control
{
    private int _caret, _anchor;
    private string _lineText = "";
    private string[] _displayLines =
    {
        ""
    };
    private bool _dragging;
    private double _blink;
    private char? _pendingSurrogate;
    private string _observed = string.Empty;
    private IControlFont? _font;
    private float _lineHeight = 16, _scrollX, _scrollY;
    private readonly Stack<(string Text, int Caret, int Anchor)> _undo = new(), _redo = new();
    /// <summary>Initializes a new text box instance.</summary>
    public TextBox() : base()
    {
    }

    /// <inheritdoc/>
    protected override string FocusVisualState => HasError ? "Focused.Invalid" : "Focused";
    /// <inheritdoc/>
    protected override string DefaultStyleName => "TextBox";
    /// <summary>Gets or sets text.</summary>
    public string Text { get => ConfigText; set => ReplaceAll(value, false); }
    /// <summary>Gets or sets is read only.</summary>
    public bool IsReadOnly { get => ConfigIsReadOnly; set => ConfigIsReadOnly = value; }
    /// <summary>Gets or sets accepts return.</summary>
    public bool AcceptsReturn { get => ConfigAcceptsReturn; set => ConfigAcceptsReturn = value; }
    /// <summary>Gets or sets accepts tab.</summary>
    public bool AcceptsTab { get => ConfigAcceptsTab; set => ConfigAcceptsTab = value; }
    /// <summary>Gets or sets is password.</summary>
    public bool IsPassword { get => ConfigIsPassword; set => ConfigIsPassword = value; }
    /// <summary>Gets or sets has error.</summary>
    public bool HasError { get => ConfigHasError; set => ConfigHasError = value; }
    /// <summary>Gets or sets placeholder.</summary>
    public string Placeholder { get => ConfigPlaceholder; set => ConfigPlaceholder = value; }
    /// <summary>Gets or sets max length.</summary>
    public int MaxLength { get => ConfigMaxLength; set => ConfigMaxLength = value; }
    /// <summary>Gets caret index.</summary>
    public int CaretIndex => _caret;
    /// <summary>Gets selection start.</summary>
    public int SelectionStart => Math.Min(_caret, _anchor);
    /// <summary>Gets selection length.</summary>
    public int SelectionLength => Math.Abs(_caret - _anchor);
    /// <summary>Gets selected text.</summary>
    public string SelectedText => Text.Substring(SelectionStart, SelectionLength);

    /// <summary>Occurs when text changed.</summary>
    public event EventHandler? TextChanged;
    /// <summary>Occurs when submitted.</summary>
    public event EventHandler? Submitted;
    /// <summary>Performs select all using the current control state.</summary>
    public void SelectAll()
    {
        _anchor = 0;
        _caret = Text.Length;
        _blink = 0;
        EnsureCaret();
    }

    /// <summary>Performs select using the current control state.</summary>
    public void Select(int start, int length)
    {
        if (start < 0 || length < 0 || (long)start + length > Text.Length)
            throw new ArgumentOutOfRangeException(nameof(start));
        _anchor = Snap(start);
        _caret = Snap(start + length);
        _blink = 0;
        EnsureCaret();
    }

    private static string Normalize(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
    private int Snap(int index)
    {
        index = Math.Clamp(index, 0, Text.Length);
        int result = 0;
        foreach (int boundary in StringInfo.ParseCombiningCharacters(Text))
        {
            if (boundary > index)
                break;
            result = boundary;
        }

        return index == Text.Length ? index : result;
    }

    private int Previous(int index)
    {
        if (index <= 0)
            return 0;
        return Snap(index - 1);
    }

    private int Next(int index)
    {
        foreach (int value in StringInfo.ParseCombiningCharacters(Text))
            if (value > index)
                return value;
        return Text.Length;
    }

    private void SaveUndo()
    {
        if (_undo.Count >= 100)
            _undo.Clear();
        _undo.Push((Text, _caret, _anchor));
        _redo.Clear();
    }

    /// <summary>Performs undo using the current control state.</summary>
    public void Undo() => Restore(_undo, _redo);
    /// <summary>Performs redo using the current control state.</summary>
    public void Redo() => Restore(_redo, _undo);
    private void Restore(Stack<(string Text, int Caret, int Anchor)> from, Stack<(string Text, int Caret, int Anchor)> to)
    {
        if (IsReadOnly || from.Count == 0)
            return;
        to.Push((Text, _caret, _anchor));
        var saved = from.Pop();
        ConfigText = saved.Text;
        _observed = saved.Text;
        _caret = saved.Caret;
        _anchor = saved.Anchor;
        _blink = 0;
        EnsureCaret();
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ReplaceAll(string value, bool record)
    {
        ArgumentNullException.ThrowIfNull(value);
        value = Normalize(value);
        if (MaxLength < 0)
            throw new InvalidOperationException("MaxLength must be nonnegative.");
        if (value.Length > MaxLength)
            throw new ArgumentException("Text exceeds MaxLength.", nameof(value));
        if (Text == value)
            return;
        if (record)
            SaveUndo();
        ConfigText = value;
        _observed = value;
        _caret = Math.Min(_caret, value.Length);
        _anchor = Math.Min(_anchor, value.Length);
        _caret = Snap(_caret);
        _anchor = Snap(_anchor);
        _blink = 0;
        EnsureCaret();
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Performs insert text using the current control state.</summary>
    public void InsertText(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (IsReadOnly)
            return;
        value = Normalize(value);
        if (!AcceptsReturn)
            value = value.Replace("\n", string.Empty, StringComparison.Ordinal);
        if (!AcceptsTab)
            value = value.Replace("\t", " ", StringComparison.Ordinal);
        int available = Math.Max(0, MaxLength - (Text.Length - SelectionLength));
        if (value.Length > available)
        {
            int end = available;
            if (end > 0 && end < value.Length && char.IsHighSurrogate(value[end - 1]))
                end--;
            value = value[..end];
        }

        if (value.Length == 0 && SelectionLength == 0)
            return;
        int start = SelectionStart;
        string next = Text[..start] + value + Text[(start + SelectionLength)..];
        SaveUndo();
        ConfigText = next;
        _observed = next;
        _caret = _anchor = start + value.Length;
        _blink = 0;
        EnsureCaret();
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Move(int index, bool extend)
    {
        _caret = Snap(index);
        if (!extend)
            _anchor = _caret;
        _blink = 0;
        EnsureCaret();
    }

    private int Word(int index, int direction)
    {
        if (direction < 0)
        {
            while (index > 0 && char.IsWhiteSpace(Text[Previous(index)]))
                index = Previous(index);
            while (index > 0 && !char.IsWhiteSpace(Text[Previous(index)]))
                index = Previous(index);
        }
        else
        {
            while (index < Text.Length && !char.IsWhiteSpace(Text[index]))
                index = Next(index);
            while (index < Text.Length && char.IsWhiteSpace(Text[index]))
                index = Next(index);
        }

        return index;
    }

    private (int Start, int End, int Line) LineAt(int index)
    {
        int start = index == 0 ? 0 : Text.LastIndexOf('\n', Math.Min(index - 1, Text.Length - 1)) + 1;
        int end = Text.IndexOf('\n', index);
        if (end < 0)
            end = Text.Length;
        int line = 0;
        for (int i = 0; i < start; i++)
            if (Text[i] == '\n')
                line++;
        return (start, end, line);
    }

    private string Visible(string value) => IsPassword ? new string ('•', StringInfo.ParseCombiningCharacters(value).Length) : value.Replace("\t", "    ", StringComparison.Ordinal);
    private float Measure(string value) => _font?.Measure(Visible(value)).Width ?? 0;
    private int Hit(float x, float y)
    {
        int line = Math.Max(0, (int)((y - ContentBounds.Y + _scrollY) / _lineHeight)), start = 0;
        for (int i = 0; i < line; i++)
        {
            int n = Text.IndexOf('\n', start);
            if (n < 0)
            {
                start = Text.Length;
                break;
            }

            start = n + 1;
        }

        int end = Text.IndexOf('\n', start);
        if (end < 0)
            end = Text.Length;
        float target = x - ContentBounds.X + _scrollX;
        int best = start;
        float previous = 0;
        for (int i = start; i < end;)
        {
            int next = Math.Min(end, Next(i));
            float width = Measure(Text[start..next]);
            if (target < (previous + width) / 2)
                return i;
            best = next;
            previous = width;
            i = next;
        }

        return best;
    }

    private void EnsureCaret()
    {
        if (_font is null)
            return;
        var line = LineAt(_caret);
        float x = Measure(Text[line.Start.._caret]), y = line.Line * _lineHeight;
        float w = MathF.Max(1, ContentBounds.Width - 2), h = MathF.Max(1, ContentBounds.Height);
        if (x < _scrollX)
            _scrollX = x;
        if (x > _scrollX + w)
            _scrollX = x - w;
        if (y < _scrollY)
            _scrollY = y;
        if (y + _lineHeight > _scrollY + h)
            _scrollY = y + _lineHeight - h;
        _scrollX = MathF.Max(0, _scrollX);
        _scrollY = AcceptsReturn ? MathF.Max(0, _scrollY) : 0;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        ArgumentNullException.ThrowIfNull(Text);
        if (MaxLength < 0)
            throw new InvalidOperationException("Invalid MaxLength.");
        _font = Font ?? context.DefaultFont;
        if (_lineText != Text)
        {
            _displayLines = Text.Split('\n');
            _lineText = Text;
        }

        float line = _font.Measure("Mg\nMg").Height - _font.Measure("Mg").Height;
        _lineHeight = MathF.Max(1, line > 0 ? line : _font.Measure("Mg").Height);
        if (_observed != Text)
        {
            _caret = Snap(Math.Min(_caret, Text.Length));
            _anchor = Snap(Math.Min(_anchor, Text.Length));
        }

        return new Size2(140, AcceptsReturn ? _lineHeight * 4 : _lineHeight);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds) => EnsureCaret();
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        _blink = (_blink + elapsed.TotalSeconds) % 1;
        if (_observed != Text)
        {
            _observed = Text;
            TextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        _dragging = false;
        _pendingSurrogate = null;
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.PointerWheel && AcceptsReturn)
        {
            float max = MathF.Max(0, _displayLines.Length * _lineHeight - ContentBounds.Height);
            _scrollY = Math.Clamp(_scrollY - e.WheelDelta / 120f * _lineHeight * 3, 0, max);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.KeyDown && (e.Alt && e.Key == Keys.Down || Parent is NumericSpinner && e.Key is Keys.Up or Keys.Down))
            return;
        if (e.Type == ControlInputEventType.KeyDown && e.Control && e.Key == Keys.Tab)
            return;
        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left)
        {
            Move(Hit(e.Position.X, e.Position.Y), e.Shift);
            _dragging = true;
            context.CapturePointer(this);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.PointerMove && _dragging)
        {
            Move(Hit(e.Position.X, e.Position.Y), true);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.PointerUp && e.Button == MouseButton.Left && _dragging)
        {
            context.ReleasePointer(this);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.TextInput)
        {
            if (e.Control && !e.Alt)
                return;
            char c = e.Character;
            if (char.IsHighSurrogate(c))
            {
                _pendingSurrogate = c;
                args.Handled = true;
                return;
            }

            if (_pendingSurrogate is char high)
            {
                _pendingSurrogate = null;
                InsertText(char.IsLowSurrogate(c) ? new string (new[] { high, c }) : "�" + c);
            }
            else if (!char.IsControl(c))
                InsertText(char.IsLowSurrogate(c) ? "�" : c.ToString());
            args.Handled = true;
            return;
        }

        if (e.Type != ControlInputEventType.KeyDown)
            return;
        if (e.Control && !e.Alt)
        {
            var clipboard = Workspace?.Registry.Clipboard;
            switch (e.Key)
            {
                case Keys.A:
                    SelectAll();
                    break;
                case Keys.C:
                    if (!IsPassword && SelectionLength > 0)
                        clipboard?.SetData("text/plain", SelectedText);
                    break;
                case Keys.X:
                    if (!IsPassword && !IsReadOnly && SelectionLength > 0)
                    {
                        clipboard?.SetData("text/plain", SelectedText);
                        InsertText(string.Empty);
                    }

                    break;
                case Keys.V:
                    if (clipboard?.GetData("text/plain")is string value)
                        InsertText(value);
                    break;
                case Keys.Z:
                    Undo();
                    break;
                case Keys.Y:
                    Redo();
                    break;
                default:
                    goto Navigation;
            }

            args.Handled = true;
            return;
        }

        Navigation:
            switch (e.Key)
            {
                case Keys.Left:
                    Move(!e.Shift && SelectionLength > 0 ? SelectionStart : e.Control ? Word(_caret, -1) : Previous(_caret), e.Shift);
                    break;
                case Keys.Right:
                    Move(!e.Shift && SelectionLength > 0 ? SelectionStart + SelectionLength : e.Control ? Word(_caret, 1) : Next(_caret), e.Shift);
                    break;
                case Keys.Home:
                    Move(e.Control ? 0 : LineAt(_caret).Start, e.Shift);
                    break;
                case Keys.End:
                    Move(e.Control ? Text.Length : LineAt(_caret).End, e.Shift);
                    break;
                case Keys.Up:
                case Keys.Down:
                    if (!AcceptsReturn)
                        return;
                    var line = LineAt(_caret);
                    Move(Hit(ContentBounds.X + Measure(Text[line.Start.._caret]) - _scrollX, ContentBounds.Y + (line.Line + (e.Key == Keys.Up ? -1 : 1)) * _lineHeight - _scrollY + .1f), e.Shift);
                    break;
                case Keys.Back:
                case Keys.Delete:
                    if (IsReadOnly)
                        break;
                    if (SelectionLength == 0)
                    {
                        _anchor = e.Key == Keys.Back ? (e.Control ? Word(_caret, -1) : Previous(_caret)) : (e.Control ? Word(_caret, 1) : Next(_caret));
                    }

                    InsertText(string.Empty);
                    break;
                case Keys.Enter:
                    if (AcceptsReturn)
                        InsertText("\n");
                    else
                    {
                        if (Submitted is not null)
                        {
                            Submitted.Invoke(this, EventArgs.Empty);
                            args.Handled = true;
                        }

                        return;
                    }

                    break;
                case Keys.Tab:
                    if (AcceptsTab)
                        InsertText("\t");
                    else
                        return;
                    break;
                default:
                    return;
            }

        args.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        string state = !IsAvailable() ? "Disabled" : HasError ? "Invalid" : IsReadOnly ? "ReadOnly" : IsFocused ? "Focused" : IsPointerOver ? "Hover" : "Normal";
        if (!context.DrawSkinPart(this, "Background", LocalBounds, state))
        {
            context.FillRectangle(LocalBounds, Background);
            if (HasError)
                context.StrokeRectangle(LocalBounds, new Color(220, 80, 90), MathF.Max(1, BorderThickness));
        }

        var clip = new Bounds2(Padding.Left, Padding.Top, ContentBounds.Width, ContentBounds.Height);
        using (context.PushClipScope(clip))
        {
            var font = Font ?? context.DefaultFont;
            if (Text.Length == 0 && Placeholder.Length > 0)
                context.DrawText(font, Placeholder, new Vector2(Padding.Left, Padding.Top), ConfigDisabledTextColor);
            string[] lines = _displayLines;
            int start = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                float y = Padding.Top + i * _lineHeight - _scrollY;
                int from = Math.Max(start, SelectionStart), to = Math.Min(start + line.Length, SelectionStart + SelectionLength);
                if (to > from)
                {
                    var selection = new Bounds2(Padding.Left + Measure(line[..(from - start)]) - _scrollX, y, Measure(line[(from - start)..(to - start)]), _lineHeight);
                    if (!context.DrawSkinPart(this, "Selection", selection))
                        context.FillRectangle(selection, new Color(35, 85, 105));
                }

                if (y + _lineHeight >= Padding.Top && y <= Padding.Top + ContentBounds.Height)
                    DrawTextLine(context, font, Visible(line), i, new Vector2(Padding.Left - _scrollX, y), IsAvailable() ? TextColor : ConfigDisabledTextColor);
                start += line.Length + 1;
            }

            if (IsFocused && !IsReadOnly && _blink < .5)
            {
                var line = LineAt(_caret);
                var caret = new Bounds2(Padding.Left + Measure(Text[line.Start.._caret]) - _scrollX, Padding.Top + line.Line * _lineHeight - _scrollY, 1, _lineHeight);
                if (!context.DrawSkinPart(this, "Caret", caret))
                    context.FillRectangle(caret, TextColor);
            }
        }
    }

    /// <summary>Draws a visible text line. Derived editors may use cached syntax runs.</summary>
    protected virtual void DrawTextLine(ControlDrawContext context, IControlFont font, string line, int lineIndex, Vector2 position, Color color) => context.DrawText(font, line, position, color);
    private StyleProperty<string> _styleText = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextChangedArgs = new(nameof(Text));
    internal string ConfigText
    {
        get => _styleText.Value;
        set
        {
            if (_styleText.SetLocal(value))
                NotifyPropertyChanged(TextChangedArgs);
        }
    }

    private StyleProperty<bool> _styleIsReadOnly = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsReadOnlyChangedArgs = new(nameof(IsReadOnly));
    internal bool ConfigIsReadOnly
    {
        get => _styleIsReadOnly.Value;
        set
        {
            if (_styleIsReadOnly.SetLocal(value))
                NotifyPropertyChanged(IsReadOnlyChangedArgs);
        }
    }

    private StyleProperty<bool> _styleAcceptsReturn = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs AcceptsReturnChangedArgs = new(nameof(AcceptsReturn));
    internal bool ConfigAcceptsReturn
    {
        get => _styleAcceptsReturn.Value;
        set
        {
            if (_styleAcceptsReturn.SetLocal(value))
                NotifyPropertyChanged(AcceptsReturnChangedArgs);
        }
    }

    private StyleProperty<bool> _styleAcceptsTab = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs AcceptsTabChangedArgs = new(nameof(AcceptsTab));
    internal bool ConfigAcceptsTab
    {
        get => _styleAcceptsTab.Value;
        set
        {
            if (_styleAcceptsTab.SetLocal(value))
                NotifyPropertyChanged(AcceptsTabChangedArgs);
        }
    }

    private StyleProperty<bool> _styleIsPassword = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsPasswordChangedArgs = new(nameof(IsPassword));
    internal bool ConfigIsPassword
    {
        get => _styleIsPassword.Value;
        set
        {
            if (_styleIsPassword.SetLocal(value))
                NotifyPropertyChanged(IsPasswordChangedArgs);
        }
    }

    private StyleProperty<int> _styleMaxLength = new(32767);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MaxLengthChangedArgs = new(nameof(MaxLength));
    internal int ConfigMaxLength
    {
        get => _styleMaxLength.Value;
        set
        {
            if (_styleMaxLength.SetLocal(value))
                NotifyPropertyChanged(MaxLengthChangedArgs);
        }
    }

    private StyleProperty<string> _stylePlaceholder = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs PlaceholderChangedArgs = new(nameof(Placeholder));
    internal string ConfigPlaceholder
    {
        get => _stylePlaceholder.Value;
        set
        {
            if (_stylePlaceholder.SetLocal(value))
                NotifyPropertyChanged(PlaceholderChangedArgs);
        }
    }

    private StyleProperty<bool> _styleHasError = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs HasErrorChangedArgs = new(nameof(HasError));
    internal bool ConfigHasError
    {
        get => _styleHasError.Value;
        set
        {
            if (_styleHasError.SetLocal(value))
                NotifyPropertyChanged(HasErrorChangedArgs);
        }
    }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "Text":
                    if (_styleText.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(TextChangedArgs);
                    break;
                case "IsReadOnly":
                    if (_styleIsReadOnly.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsReadOnlyChangedArgs);
                    break;
                case "AcceptsReturn":
                    if (_styleAcceptsReturn.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(AcceptsReturnChangedArgs);
                    break;
                case "AcceptsTab":
                    if (_styleAcceptsTab.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(AcceptsTabChangedArgs);
                    break;
                case "IsPassword":
                    if (_styleIsPassword.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsPasswordChangedArgs);
                    break;
                case "MaxLength":
                    if (_styleMaxLength.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(MaxLengthChangedArgs);
                    break;
                case "Placeholder":
                    if (_stylePlaceholder.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(PlaceholderChangedArgs);
                    break;
                case "HasError":
                    if (_styleHasError.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(HasErrorChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Text":
                return _styleText.HasLocalValue;
            case "IsReadOnly":
                return _styleIsReadOnly.HasLocalValue;
            case "AcceptsReturn":
                return _styleAcceptsReturn.HasLocalValue;
            case "AcceptsTab":
                return _styleAcceptsTab.HasLocalValue;
            case "IsPassword":
                return _styleIsPassword.HasLocalValue;
            case "MaxLength":
                return _styleMaxLength.HasLocalValue;
            case "Placeholder":
                return _stylePlaceholder.HasLocalValue;
            case "HasError":
                return _styleHasError.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Text":
                if (_styleText.ClearLocal())
                    NotifyPropertyChanged(TextChangedArgs);
                return;
            case "IsReadOnly":
                if (_styleIsReadOnly.ClearLocal())
                    NotifyPropertyChanged(IsReadOnlyChangedArgs);
                return;
            case "AcceptsReturn":
                if (_styleAcceptsReturn.ClearLocal())
                    NotifyPropertyChanged(AcceptsReturnChangedArgs);
                return;
            case "AcceptsTab":
                if (_styleAcceptsTab.ClearLocal())
                    NotifyPropertyChanged(AcceptsTabChangedArgs);
                return;
            case "IsPassword":
                if (_styleIsPassword.ClearLocal())
                    NotifyPropertyChanged(IsPasswordChangedArgs);
                return;
            case "MaxLength":
                if (_styleMaxLength.ClearLocal())
                    NotifyPropertyChanged(MaxLengthChangedArgs);
                return;
            case "Placeholder":
                if (_stylePlaceholder.ClearLocal())
                    NotifyPropertyChanged(PlaceholderChangedArgs);
                return;
            case "HasError":
                if (_styleHasError.ClearLocal())
                    NotifyPropertyChanged(HasErrorChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleText.BeginStyle(string.Empty))
            NotifyPropertyChanged(TextChangedArgs);
        if (_styleIsReadOnly.BeginStyle(false))
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleAcceptsReturn.BeginStyle(false))
            NotifyPropertyChanged(AcceptsReturnChangedArgs);
        if (_styleAcceptsTab.BeginStyle(false))
            NotifyPropertyChanged(AcceptsTabChangedArgs);
        if (_styleIsPassword.BeginStyle(false))
            NotifyPropertyChanged(IsPasswordChangedArgs);
        if (_styleMaxLength.BeginStyle(32767))
            NotifyPropertyChanged(MaxLengthChangedArgs);
        if (_stylePlaceholder.BeginStyle(string.Empty))
            NotifyPropertyChanged(PlaceholderChangedArgs);
        if (_styleHasError.BeginStyle(false))
            NotifyPropertyChanged(HasErrorChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleText.CommitStyle())
            NotifyPropertyChanged(TextChangedArgs);
        if (_styleIsReadOnly.CommitStyle())
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleAcceptsReturn.CommitStyle())
            NotifyPropertyChanged(AcceptsReturnChangedArgs);
        if (_styleAcceptsTab.CommitStyle())
            NotifyPropertyChanged(AcceptsTabChangedArgs);
        if (_styleIsPassword.CommitStyle())
            NotifyPropertyChanged(IsPasswordChangedArgs);
        if (_styleMaxLength.CommitStyle())
            NotifyPropertyChanged(MaxLengthChangedArgs);
        if (_stylePlaceholder.CommitStyle())
            NotifyPropertyChanged(PlaceholderChangedArgs);
        if (_styleHasError.CommitStyle())
            NotifyPropertyChanged(HasErrorChangedArgs);
    }
}
