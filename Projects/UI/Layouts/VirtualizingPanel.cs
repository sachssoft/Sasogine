using Sachssoft.Engine;

namespace Sachssoft.Engine.Controls.Layouts;

/// <summary>Computes visible slots for an indexed items host with uniform vertical rows.</summary>
public abstract class VirtualizingPanel : ContainerControl
{
    /// <summary>Stores the layout context used to arrange realized containers.</summary>
    private ControlLayoutContext? _layout;

    /// <summary>Stores the number of columns used by the last arranged viewport.</summary>
    private int _lastColumns;

    /// <summary>Stores the first visible item across viewport changes.</summary>
    private int _anchorIndex;

    /// <summary>Stores the fractional scroll position within the anchor row.</summary>
    private float _anchorFraction;

    /// <summary>Stores the row stride used by the last arranged viewport.</summary>
    private float _lastStride;
    private Bounds2 _arrangedOrigin;
    private Size2 _arrangedSlot;

    /// <summary>Gets or sets the owning items control.</summary>
    internal ItemsControl? ItemsOwner { get; set; }

    /// <summary>Gets the column count used by keyboard navigation after arrangement.</summary>
    internal int ArrangedColumns => Math.Max(1, _lastColumns);

    /// <summary>Gets the arranged distance between the starts of neighboring rows.</summary>
    internal float ArrangedRowStride => _lastStride;

    /// <summary>Gets the logical gap between slots.</summary>
    public float Spacing { get; init; }

    /// <summary>Gets the number of extra rows realized on each side of the viewport.</summary>
    public int OverscanRows { get; init; } = 2;

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;

    /// <summary>Calculates the uniform slot size and column count for the supplied width.</summary>
    /// <param name="width">The available viewport width.</param>
    /// <param name="columns">The positive number of columns.</param>
    /// <returns>The size of a single slot.</returns>
    protected abstract Size2 GetSlot(float width, out int columns);

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _layout = context;
        float width = float.IsFinite(available.Width) ? available.Width : 0;
        Size2 slot = GetValidatedSlot(width, out int columns);
        int items = ItemsOwner?.ItemCount ?? 0;
        int rows = items == 0 ? 0 : (items - 1) / columns + 1;
        double extent = rows == 0 ? 0 : (double)rows * (slot.Height + Spacing) - Spacing;
        if (extent > float.MaxValue)
            throw new InvalidOperationException("The virtual item extent is too large.");
        return new Size2(width, (float)extent);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        if (_layout is not { } layout || ItemsOwner is not { } owner || Parent is not ScrollViewer viewer)
            return;

        Size2 slot = GetValidatedSlot(bounds.Width, out int columns);
        int items = owner.ItemCount;
        int rows = items == 0 ? 0 : (items - 1) / columns + 1;
        double stride = slot.Height + Spacing;
        float offset = viewer.VerticalScrollBar.Value;
        float origin = bounds.Y;
        if (items > 0 && _lastColumns != 0 && columns != _lastColumns)
        {
            viewer.VerticalScrollBar.Value = (float)Math.Clamp(
                (Math.Min(_anchorIndex, items - 1) / columns + (double)_anchorFraction) * stride,
                viewer.VerticalScrollBar.Minimum, viewer.VerticalScrollBar.Maximum);
            float adjusted = viewer.VerticalScrollBar.Value;
            origin += offset - adjusted;
            offset = adjusted;
        }

        _lastColumns = columns;
        _lastStride = (float)stride;
        _arrangedOrigin = new Bounds2(bounds.X, origin, bounds.Width, bounds.Height);
        _arrangedSlot = slot;
        int visibleRow = (int)Math.Clamp(Math.Floor(offset / stride), 0, Math.Max(0, rows - 1));
        _anchorIndex = visibleRow * columns;
        _anchorFraction = (float)(offset / stride - visibleRow);
        double viewportHeight = viewer.ViewportBounds.Height;
        int bufferedRows = viewportHeight <= 0 ? 0 : (int)Math.Min(rows,
            Math.Ceiling(viewportHeight / stride) + 1 + 2d * OverscanRows);
        int firstRow = Math.Clamp(visibleRow - OverscanRows, 0, rows - bufferedRows);
        int count = (int)Math.Min(items, (long)bufferedRows * columns);
        int first = Math.Min(firstRow * columns, items - count);
        owner.RealizeVirtualRange(this, first, count);
        for (int index = first; index < first + count; index++)
        {
            Control? item = owner.GetVirtualPresentation(index);
            if (item is null || !ReferenceEquals(item.CommittedParent, this))
                continue;
            item.Measure(layout, slot);
            item.Arrange(GetItemBounds(index), true);
        }
    }

    /// <summary>Returns the slot for any source index using the last arranged geometry.</summary>
    /// <param name="index">The source index.</param>
    /// <returns>The slot in workspace coordinates.</returns>
    internal Bounds2 GetItemBounds(int index) => _lastColumns <= 0 ? Bounds2.Zero :
        new Bounds2(_arrangedOrigin.X + index % _lastColumns * (_arrangedSlot.Width + Spacing),
            _arrangedOrigin.Y + (float)(index / _lastColumns * (double)_lastStride), _arrangedSlot.Width, _arrangedSlot.Height);

    /// <summary>Scrolls an indexed item into the arranged viewport.</summary>
    /// <param name="index">The item to reveal.</param>
    internal void Reveal(int index)
    {
        if (_lastColumns <= 0 || Parent is not ScrollViewer viewer)
            return;
        float top = index / _lastColumns * _lastStride;
        float bottom = top + _lastStride - Spacing;
        float offset = viewer.VerticalScrollBar.Value;
        float height = viewer.ViewportBounds.Height;
        if (top < offset || bottom > offset + height)
            viewer.VerticalScrollBar.Value = top < offset ? top : bottom - height;
    }

    /// <summary>Validates layout settings and obtains the slot geometry.</summary>
    /// <param name="width">The available viewport width.</param>
    /// <param name="columns">The computed column count.</param>
    /// <returns>The validated slot size.</returns>
    private Size2 GetValidatedSlot(float width, out int columns)
    {
        Size2 slot = GetSlot(width, out columns);
        if (columns < 1 || OverscanRows < 0 || !float.IsFinite(Spacing) || Spacing < 0 ||
            !float.IsFinite(slot.Width + Spacing) || slot.Width < 0 ||
            !float.IsFinite(slot.Height + Spacing) || slot.Height <= 0)
            throw new InvalidOperationException("Invalid virtual items panel geometry.");
        return slot;
    }
}
