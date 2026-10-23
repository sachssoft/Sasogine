using System.ComponentModel;
using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>
/// Edits a typed value using explicit formatting and parsing; commits on Enter or focus loss.
/// </summary>
/// <typeparam name = "T">The value type.</typeparam>
public sealed class TypedValueTextBox<T> : TextBox
{
    private readonly Func<T, string> _format;
    private readonly TryParseInspectorValue<T> _parse;
    private static readonly PropertyChangedEventArgs ValueArgs = new(nameof(Value));
    private T _value;
    private bool _syncing, _dirty;
    /// <summary>
    /// Creates an editor with a formatter and parser that work under NativeAOT.
    /// </summary>
    public TypedValueTextBox(T value, Func<T, string> format, TryParseInspectorValue<T> parse)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(parse);
        _format = format;
        _parse = parse;
        _value = value;
        Height = 32;
        Sync();
        TextChanged += (_, _) =>
        {
            if (!_syncing)
            {
                _dirty = true;
                HasError = !_parse(Text, out _);
            }
        };
        Submitted += (_, _) => CommitEdit();
    }

    /// <summary>
    /// Gets or sets the last committed value, preserving text currently being edited.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (SetNotify(ref _value, value, ValueArgs))
            {
                if (!IsFocused)
                    Sync();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Occurs after a committed value changes.
    /// </summary>
    public event EventHandler? ValueChanged;
    /// <summary>
    /// Validates and commits pending text; invalid input preserves the source.
    /// </summary>
    public bool CommitEdit()
    {
        if (!_dirty)
            return true;
        if (IsReadOnly || !_parse(Text, out T value))
        {
            HasError = true;
            return false;
        }

        T previous = _value;
        string attempted = Text;
        try
        {
            Value = value;
            Sync();
            return true;
        }
        catch (Exception error)when (error is ArgumentException or OverflowException)
        {
            _value = previous;
            _syncing = true;
            try
            {
                Text = attempted;
                _dirty = true;
                HasError = true;
                NotifyPropertyChanged(ValueArgs);
            }
            finally
            {
                _syncing = false;
            }

            return false;
        }
    }

    /// <summary>
    /// Discards pending text and displays the latest committed source value.
    /// </summary>
    public void CancelEdit() => Sync();
    private void Sync()
    {
        _syncing = true;
        try
        {
            Text = _format(_value);
            _dirty = false;
            HasError = false;
        }
        finally
        {
            _syncing = false;
        }
    }

    /// <inheritdoc/>
    protected override void OnFocusLost()
    {
        if (!CommitEdit())
            CancelEdit();
        base.OnFocusLost();
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        if (args.Input.Type == ControlInputEventType.KeyDown && args.Input.Key == Keys.Escape)
        {
            CancelEdit();
            args.Handled = true;
            return;
        }

        base.OnInput(context, args);
    }
}
