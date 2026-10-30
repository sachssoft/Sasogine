using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Resizes the two grid dimensions immediately before and after its own cell.
/// </summary>
/// <remarks>
/// Place the splitter in a dedicated row or column of its direct parent grid.
/// Horizontal direction changes column widths; vertical direction changes row heights.
/// Automatic dimensions become fixed sizes after editing. Proportional dimensions
/// retain proportional sizing. Escape cancels the current pointer gesture.
/// </remarks>
public sealed class GridSplitter : Control
{
    /// <summary>Cached notification for the resizing direction.</summary>
    private static readonly PropertyChangedEventArgs DirectionChangedArgs = new(nameof(Direction));

    /// <summary>Cached notification for pointer editing state.</summary>
    private static readonly PropertyChangedEventArgs IsDraggingChangedArgs = new(nameof(IsDragging));

    /// <summary>Cached notification for the keyboard increment.</summary>
    private static readonly PropertyChangedEventArgs KeyboardIncrementChangedArgs = new(nameof(KeyboardIncrement));

    /// <summary>Cached notification for the interactive pane minimum.</summary>
    private static readonly PropertyChangedEventArgs MinimumPaneSizeChangedArgs = new(nameof(MinimumPaneSize));

    /// <summary>The direction captured when editing begins.</summary>
    private ControlOrientation _activeDirection;

    /// <summary>The configured resize direction.</summary>
    private ControlOrientation _direction = ControlOrientation.Horizontal;

    /// <summary>The captured pointer identity.</summary>
    private ControlPointer? _pointer;

    /// <summary>The grid owning the edited dimensions.</summary>
    private Grid? _grid;

    /// <summary>The leading dimension being resized.</summary>
    private GridDimension? _first;

    /// <summary>The trailing dimension being resized.</summary>
    private GridDimension? _second;

    /// <summary>The original leading sizing rule.</summary>
    private GridLength _firstLength;

    /// <summary>The last leading sizing rule written by this gesture.</summary>
    private GridLength _lastFirst;

    /// <summary>The last trailing sizing rule written by this gesture.</summary>
    private GridLength _lastSecond;

    /// <summary>The original trailing sizing rule.</summary>
    private GridLength _secondLength;

    /// <summary>The leading pane's arranged size at the start of editing.</summary>
    private float _firstSize;

    /// <summary>The keyboard increment in logical pixels.</summary>
    private float _keyboardIncrement = 10f;

    /// <summary>The minimum size enforced on each pane during editing.</summary>
    private float _minimumPaneSize = 48f;

    /// <summary>The trailing pane's arranged size at the start of editing.</summary>
    private float _secondSize;

    /// <summary>The pointer coordinate at the start of editing.</summary>
    private float _startPosition;

    /// <summary>Gets or sets the axis along which adjacent panes are resized.</summary>
    public ControlOrientation Direction
    {
        get => _direction;
        set
        {
            if (value is not (ControlOrientation.Horizontal or ControlOrientation.Vertical))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_direction == value)
                return;
            Stop(true);
            SetNotify(ref _direction, value, DirectionChangedArgs);
        }
    }

    /// <summary>Gets whether a pointer gesture is currently resizing the grid.</summary>
    public bool IsDragging => _pointer.HasValue;

    /// <summary>Gets or sets the positive arrow-key increment in logical pixels. Shift multiplies it by ten.</summary>
    public float KeyboardIncrement
    {
        get => _keyboardIncrement;
        set
        {
            if (!float.IsFinite(value) || value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _keyboardIncrement, value, KeyboardIncrementChangedArgs);
        }
    }

    /// <summary>Gets or sets the nonnegative interactive minimum for both panes, in addition to their grid limits.</summary>
    public float MinimumPaneSize
    {
        get => _minimumPaneSize;
        set
        {
            if (!float.IsFinite(value) || value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _minimumPaneSize, value, MinimumPaneSizeChangedArgs);
        }
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(GridSplitter);

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        return Direction == ControlOrientation.Horizontal ? new Size2(6, 24) : new Size2(24, 6);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var input = args.Input;
        if (input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Escape && IsDragging)
        {
            Stop(true);
            args.Handled = true;
            return;
        }

        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left && !IsDragging)
        {
            if (!BeginResize())
                return;
            _startPosition = Coordinate(input.Position);
            context.Focus(this);
            if (!IsAvailable() || !HasSamePanes())
            {
                Stop(true);
                return;
            }
            _pointer = input.Pointer;
            context.CapturePointer(this);
            NotifyPropertyChanged(IsDraggingChangedArgs);
            InvalidateVisual();
            args.Handled = true;
        }
        else if (IsDragging && input.Pointer == _pointer)
        {
            if (input.Type == ControlInputEventType.PointerMove)
            {
                Resize(Coordinate(input.Position) - _startPosition);
                args.Handled = true;
            }
            else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
            {
                Resize(Coordinate(input.Position) - _startPosition);
                Stop(false);
                args.Handled = true;
            }
            else if (input.Type == ControlInputEventType.PointerCancel)
            {
                Stop(true);
                args.Handled = true;
            }
        }
        else if (input.Type == ControlInputEventType.KeyDown && IsFocused)
        {
            bool horizontal = Direction == ControlOrientation.Horizontal;
            int sign = input.Key == (horizontal ? Keys.Left : Keys.Up) ? -1 :
                input.Key == (horizontal ? Keys.Right : Keys.Down) ? 1 : 0;
            if (sign != 0 && BeginResize())
            {
                Resize(sign * KeyboardIncrement * (input.Shift ? 10 : 1));
                Stop(false);
                args.Handled = true;
            }
        }
        else
        {
            base.OnInput(context, args);
        }
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        Stop(true);
        base.OnInteractionCancelled();
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(ControlPointer pointer)
    {
        if (_pointer == pointer)
            Stop(true);
        base.OnPointerCaptureLost(pointer);
    }

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        Stop(true);
        base.OnDetached();
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        bool horizontal = Direction == ControlOrientation.Horizontal;
        float length = MathF.Min(24, horizontal ? ContentBounds.Height : ContentBounds.Width);
        var grip = horizontal
            ? new Bounds2(Padding.Left + ContentBounds.Width / 2 - 1, Padding.Top + (ContentBounds.Height - length) / 2, 2, length)
            : new Bounds2(Padding.Left + (ContentBounds.Width - length) / 2, Padding.Top + ContentBounds.Height / 2 - 1, length, 2);
        context.FillRectangle(grip, IsAvailable() ? TextColor : TextColor * 0.4f);
    }

    /// <summary>Captures the neighboring dimensions and their current arranged sizes.</summary>
    /// <returns>Whether this cell has two visible, finite neighboring panes.</returns>
    private bool BeginResize()
    {
        if (Parent is not Grid grid || !grid.Cells.TryGetValue(this, out var cell))
            return false;
        bool horizontal = Direction == ControlOrientation.Horizontal;
        int index = horizontal ? cell.Column : cell.Row;
        int count = horizontal ? grid.Columns.Count : grid.Rows.Count;
        if (index < 1 || index + 1 >= count || (horizontal ? cell.ColumnSpan : cell.RowSpan) != 1)
            return false;

        _firstSize = horizontal ? grid.GetColumnWidth(index - 1) : grid.GetRowHeight(index - 1);
        _secondSize = horizontal ? grid.GetColumnWidth(index + 1) : grid.GetRowHeight(index + 1);
        if (!float.IsFinite(_firstSize + _secondSize) || _firstSize <= 0 || _secondSize <= 0)
            return false;
        _grid = grid;
        _activeDirection = Direction;
        _first = horizontal ? grid.Columns[index - 1] : grid.Rows[index - 1];
        _second = horizontal ? grid.Columns[index + 1] : grid.Rows[index + 1];
        _firstLength = _lastFirst = _first.Length;
        _secondLength = _lastSecond = _second.Length;
        return true;
    }

    /// <summary>Checks that the original neighboring dimensions still belong to this splitter.</summary>
    /// <returns>Whether the captured grid relationship remains valid.</returns>
    private bool HasSamePanes()
    {
        if (_grid is null || Parent != _grid || Direction != _activeDirection ||
            !_grid.Cells.TryGetValue(this, out var cell))
            return false;
        bool horizontal = _activeDirection == ControlOrientation.Horizontal;
        int index = horizontal ? cell.Column : cell.Row;
        int count = horizontal ? _grid.Columns.Count : _grid.Rows.Count;
        return index > 0 && index + 1 < count &&
            (horizontal ? cell.ColumnSpan : cell.RowSpan) == 1 &&
            ReferenceEquals(_first, horizontal ? (GridDimension)_grid.Columns[index - 1] : _grid.Rows[index - 1]) &&
            ReferenceEquals(_second, horizontal ? (GridDimension)_grid.Columns[index + 1] : _grid.Rows[index + 1]);
    }

    /// <summary>Applies a bounded displacement while retaining proportional sizing rules.</summary>
    /// <param name="delta">Displacement from the start of the edit in logical pixels.</param>
    private void Resize(float delta)
    {
        if (!HasSamePanes() || _first is null || _second is null ||
            _first.Length != _lastFirst || _second.Length != _lastSecond)
        {
            Stop(false);
            return;
        }
        float total = _firstSize + _secondSize;
        float minimum = MathF.Max(MathF.Max(MinimumPaneSize, _first.Minimum), total - _second.Maximum);
        float maximum = MathF.Min(_first.Maximum, total - MathF.Max(MinimumPaneSize, _second.Minimum));
        if (!float.IsFinite(delta) || !float.IsFinite(minimum) || !float.IsFinite(maximum) || minimum > maximum)
            return;
        float firstSize = Math.Clamp(_firstSize + delta, minimum, maximum);
        float secondSize = total - firstSize;
        GridLength first = new(firstSize);
        GridLength second = new(secondSize);
        if (_firstLength.Unit == GridUnitType.Proportion && _secondLength.Unit == GridUnitType.Proportion)
        {
            double weight = Math.Min(float.MaxValue, (double)_firstLength.Value + _secondLength.Value);
            first = GridLength.Proportion((float)(weight * firstSize / total));
            second = GridLength.Proportion((float)(weight * secondSize / total));
        }
        else if (_firstLength.Unit == GridUnitType.Proportion)
        {
            first = _firstLength;
        }
        else if (_secondLength.Unit == GridUnitType.Proportion)
        {
            second = _secondLength;
        }
        GridDimension firstDimension = _first;
        GridDimension secondDimension = _second;
        GridLength previousSecond = _lastSecond;
        _lastFirst = first;
        firstDimension.Length = first;
        if (HasSamePanes() && firstDimension.Length == first && secondDimension.Length == previousSecond)
        {
            _lastSecond = second;
            secondDimension.Length = second;
        }
        else
        {
            Stop(false);
        }
    }

    /// <summary>Ends an edit and optionally restores sizing rules untouched by outside code.</summary>
    /// <param name="cancel">Whether to restore the captured sizing rules.</param>
    private void Stop(bool cancel)
    {
        var pointer = _pointer;
        var first = _first;
        var second = _second;
        bool restore = cancel && HasSamePanes() && first is not null && second is not null &&
            first.Length == _lastFirst && second.Length == _lastSecond;
        GridLength firstLength = _firstLength;
        GridLength secondLength = _secondLength;
        GridLength lastSecond = _lastSecond;
        _pointer = null;
        _first = null;
        _second = null;
        _grid = null;
        if (pointer is ControlPointer captured)
            Workspace?.ReleasePointer(this, captured);
        if (restore && first is not null && second is not null)
        {
            first.Length = firstLength;
            if (second.Length == lastSecond)
                second.Length = secondLength;
        }
        if (pointer.HasValue)
            NotifyPropertyChanged(IsDraggingChangedArgs);
        InvalidateVisual();
    }

    /// <summary>Extracts a pointer coordinate along the resize axis.</summary>
    /// <param name="position">The pointer position in workspace logical coordinates.</param>
    /// <returns>The horizontal or vertical coordinate.</returns>
    private float Coordinate(Vector2 position)
    {
        return _activeDirection == ControlOrientation.Horizontal ? position.X : position.Y;
    }
}
