using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Layouts;

/// <summary>Fits uniform, optionally aspect-preserving tiles to the available wrapping axis.</summary>
/// <remarks>
/// Incomplete final lines retain the same tile size. Hidden children consume no cells.
/// Minimum sizes may overflow a smaller viewport. An unbounded wrapping axis uses
/// preferred tiles on a single line. This panel does not virtualize its children.
/// </remarks>
public sealed class AdaptiveTilePanel : ContainerControl
{
    /// <summary>Caches the notification for the last arranged tile size.</summary>
    private static readonly PropertyChangedEventArgs ActualTileSizeArgs = new(nameof(ActualTileSize));

    /// <summary>Caches the effective MinTileSize notification.</summary>
    private static readonly PropertyChangedEventArgs MinTileSizeArgs = new(nameof(MinTileSize));

    /// <summary>Caches the effective Orientation notification.</summary>
    private static readonly PropertyChangedEventArgs OrientationArgs = new(nameof(Orientation));

    /// <summary>Caches the effective PreferredTileSize notification.</summary>
    private static readonly PropertyChangedEventArgs PreferredTileSizeArgs = new(nameof(PreferredTileSize));

    /// <summary>Caches the effective PreserveAspectRatio notification.</summary>
    private static readonly PropertyChangedEventArgs PreserveAspectRatioArgs = new(nameof(PreserveAspectRatio));

    /// <summary>Caches the effective Spacing notification.</summary>
    private static readonly PropertyChangedEventArgs SpacingArgs = new(nameof(Spacing));

    /// <summary>Retains the measurement context for a changed final arrangement constraint.</summary>
    private ControlLayoutContext? _layoutContext;

    /// <summary>Stores the last arranged size exposed to callers.</summary>
    private Size2 _actualTileSize;

    /// <summary>Stores the tile constraint used by the latest successful child measurement.</summary>
    private Size2 _measuredTileSize;

    /// <summary>Stores the styled MinTileSize and its optional local override.</summary>
    private StyleProperty<Size2> _minTileSize = new(new Size2(16));

    /// <summary>Stores the styled PreferredTileSize and its optional local override.</summary>
    private StyleProperty<Size2> _preferredTileSize = new(new Size2(128));

    /// <summary>Stores the styled Orientation and its optional local override.</summary>
    private StyleProperty<StackOrientation> _orientation = new(StackOrientation.Horizontal);

    /// <summary>Stores the styled PreserveAspectRatio and its optional local override.</summary>
    private StyleProperty<bool> _preserveAspectRatio = new(true);

    /// <summary>Stores the styled Spacing and its optional local override.</summary>
    private StyleProperty<float> _spacing = new(8);

    /// <summary>Gets the logical tile slot size from the most recent arrangement, or zero when empty.</summary>
    /// <remarks>Child margins and explicit child sizes can reduce the displayed content inside a slot.</remarks>
    public Size2 ActualTileSize => _actualTileSize;

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(AdaptiveTilePanel);

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;

    /// <summary>Gets or sets the minimum logical tile dimensions. Both components must be finite and positive. Defaults to 16 by 16.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The value is outside the documented range.</exception>
    public Size2 MinTileSize
    {
        get => _minTileSize.Value;
        set
        {
            ValidateTileSize(value);
            if (_minTileSize.SetLocal(value))
                NotifyPropertyChanged(MinTileSizeArgs);
        }
    }

    /// <summary>Gets or sets the wrapping direction. Horizontal fills rows; Vertical fills columns. Defaults to Horizontal.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The value is outside the documented range.</exception>
    public StackOrientation Orientation
    {
        get => _orientation.Value;
        set
        {
            ValidateOrientation(value);
            if (_orientation.SetLocal(value))
                NotifyPropertyChanged(OrientationArgs);
        }
    }

    /// <summary>Gets or sets the preferred logical tile dimensions and, when enabled, their aspect ratio. Defaults to 128 by 128.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The value is outside the documented range.</exception>
    public Size2 PreferredTileSize
    {
        get => _preferredTileSize.Value;
        set
        {
            ValidateTileSize(value);
            if (_preferredTileSize.SetLocal(value))
                NotifyPropertyChanged(PreferredTileSizeArgs);
        }
    }

    /// <summary>Gets or sets whether resizing preserves the preferred aspect ratio. Defaults to true; both minimum dimensions still apply.</summary>
    public bool PreserveAspectRatio
    {
        get => _preserveAspectRatio.Value;
        set
        {
            if (_preserveAspectRatio.SetLocal(value))
                NotifyPropertyChanged(PreserveAspectRatioArgs);
        }
    }

    /// <summary>Gets or sets the nonnegative finite logical gap between tiles. Defaults to 8; there is no outer gap.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The value is outside the documented range.</exception>
    public float Spacing
    {
        get => _spacing.Value;
        set
        {
            ValidateSpacing(value);
            if (_spacing.SetLocal(value))
                NotifyPropertyChanged(SpacingArgs);
        }
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        int count = CountVisibleChildren();
        Size2 tile = CalculateTileSize(available, count, out int cells);
        Size2 desired = CalculateDesiredSize(tile, cells, count, available);
        MeasureChildren(context, tile);
        _layoutContext = context;
        _measuredTileSize = tile;
        return desired;
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        int count = CountVisibleChildren();
        Size2 tile = CalculateTileSize(new Size2(bounds.Width, bounds.Height), count, out int cells);
        _ = CalculateDesiredSize(tile, cells, count, new Size2(bounds.Width, bounds.Height));
        if (_measuredTileSize != tile && _layoutContext is not null)
        {
            MeasureChildren(_layoutContext, tile);
            _measuredTileSize = tile;
        }

        bool horizontal = Orientation == StackOrientation.Horizontal;
        int index = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
            {
                child.Arrange(Bounds2.Zero);
                continue;
            }

            int line = index / cells;
            int cell = index % cells;
            int column = horizontal ? cell : line;
            int row = horizontal ? line : cell;
            float x = bounds.X + ToExtent(column * ((double)tile.Width + Spacing));
            float y = bounds.Y + ToExtent(row * ((double)tile.Height + Spacing));
            child.Arrange(new Bounds2(x, y, tile.Width, tile.Height), true);
            index++;
        }

        SetNotify(ref _actualTileSize, tile, ActualTileSizeArgs);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName != nameof(ActualTileSize))
            base.OnPropertyChanged(notification);
    }

    /// <summary>Counts visible children without allocating a temporary collection.</summary>
    /// <returns>The number of children occupying tile slots.</returns>
    private int CountVisibleChildren()
    {
        int count = 0;
        foreach (Control child in Children)
            if (child.IsVisible)
                count++;
        return count;
    }

    /// <summary>Measures retained children against the common tile constraint.</summary>
    /// <param name = "context">The current measurement context.</param>
    /// <param name = "tile">The common tile slot size.</param>
    private void MeasureChildren(ControlLayoutContext context, Size2 tile)
    {
        foreach (Control child in Children)
            child.Measure(context, child.IsVisible ? tile : Size2.Zero);
    }

    /// <summary>Calculates a uniform tile size, respecting minimum dimensions and orientation.</summary>
    /// <param name = "available">The available content area.</param>
    /// <param name = "count">The number of visible children.</param>
    /// <param name = "cells">Receives the number of available slots per line, or one when empty.</param>
    /// <returns>The tile size, or zero when empty.</returns>
    private Size2 CalculateTileSize(Size2 available, int count, out int cells)
    {
        cells = 1;
        if (count == 0)
            return Size2.Zero;
        bool horizontal = Orientation == StackOrientation.Horizontal;
        Size2 preferred = PreferredTileSize;
        Size2 minimum = MinTileSize;
        double scale = PreserveAspectRatio ? Math.Max(1, Math.Max((double)minimum.Width / preferred.Width, (double)minimum.Height / preferred.Height)) : 1;
        double width = PreserveAspectRatio ? preferred.Width * scale : Math.Max(preferred.Width, minimum.Width);
        double height = PreserveAspectRatio ? preferred.Height * scale : Math.Max(preferred.Height, minimum.Height);
        double axis = horizontal ? available.Width : available.Height;
        if (!double.IsFinite(axis))
            cells = count;
        else
        {
            double preferredMain = horizontal ? width : height;
            cells = (int)Math.Clamp(Math.Floor((axis + Spacing) / (preferredMain + Spacing)), 1, int.MaxValue);
            double ratio = (double)preferred.Width / preferred.Height;
            double minimumMain = horizontal ? minimum.Width : minimum.Height;
            if (PreserveAspectRatio)
                minimumMain = horizontal ? Math.Max(minimumMain, minimum.Height * ratio) : Math.Max(minimumMain, minimum.Width / ratio);
            double main = Math.Max(minimumMain, (axis - (cells - 1d) * Spacing) / cells);
            if (horizontal)
            {
                width = main;
                if (PreserveAspectRatio)
                    height = main / ratio;
            }
            else
            {
                height = main;
                if (PreserveAspectRatio)
                    width = main * ratio;
            }
        }

        return new Size2(ToExtent(width), ToExtent(height));
    }

    /// <summary>Calculates the full content extent, including gaps only between slots.</summary>
    /// <param name = "tile">The uniform tile dimensions.</param>
    /// <param name = "cells">The number of available slots per line.</param>
    /// <param name = "count">The number of visible children.</param>
    /// <param name = "available">The available area used to resolve the wrapping axis exactly.</param>
    /// <returns>The finite content extent, or zero when empty.</returns>
    private Size2 CalculateDesiredSize(Size2 tile, int cells, int count, Size2 available)
    {
        if (count == 0)
            return Size2.Zero;
        int lines = (count - 1) / cells + 1;
        int columns = Orientation == StackOrientation.Horizontal ? cells : lines;
        int rows = Orientation == StackOrientation.Horizontal ? lines : cells;
        double width = columns * (double)tile.Width + (columns - 1d) * Spacing;
        double height = rows * (double)tile.Height + (rows - 1d) * Spacing;
        // Avoid a phantom overflow caused by summing rounded Single-precision tile sizes.
        if (Orientation == StackOrientation.Horizontal && float.IsFinite(available.Width))
            width = Math.Max(available.Width, tile.Width);
        if (Orientation == StackOrientation.Vertical && float.IsFinite(available.Height))
            height = Math.Max(available.Height, tile.Height);
        return new Size2(ToExtent(width), ToExtent(height));
    }

    /// <summary>Rejects unrepresentable layout extents rather than propagating infinity.</summary>
    /// <param name = "value">The nonnegative extent in logical pixels.</param>
    /// <returns>The representable extent.</returns>
    /// <exception cref = "InvalidOperationException">The layout extent exceeds finite Single precision range.</exception>
    private static float ToExtent(double value)
    {
        if (!double.IsFinite(value) || value < 0 || value > float.MaxValue)
            throw new InvalidOperationException("The adaptive tile layout exceeds the supported coordinate range.");
        return (float)value;
    }

    /// <summary>Validates a positive, finite preferred or minimum tile size.</summary>
    /// <param name = "value">The logical tile dimensions.</param>
    private static void ValidateTileSize(Size2 value)
    {
        if (!float.IsFinite(value.Width) || !float.IsFinite(value.Height) || value.Width <= 0 || value.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Tile dimensions must be finite and positive.");
    }

    /// <summary>Validates the supported wrapping orientations.</summary>
    /// <param name = "value">The requested wrapping direction.</param>
    private static void ValidateOrientation(StackOrientation value)
    {
        if (value is not (StackOrientation.Horizontal or StackOrientation.Vertical))
            throw new ArgumentOutOfRangeException(nameof(value));
    }

    /// <summary>Rejects invalid tile gaps before they enter layout.</summary>
    /// <param name = "value">The logical tile gap.</param>
    private static void ValidateSpacing(float value)
    {
        if (!float.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Spacing must be finite and nonnegative.");
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(MinTileSize):
                {
                    Size2 value = Converters.Convert<Size2>("Size2", entry.Value);
                    ValidateTileSize(value);
                    if (_minTileSize.Apply(value))
                        NotifyPropertyChanged(MinTileSizeArgs);
                    break;
                }

                case nameof(Orientation):
                {
                    StackOrientation value = Converters.Convert<StackOrientation>("StackOrientation", entry.Value);
                    ValidateOrientation(value);
                    if (_orientation.Apply(value))
                        NotifyPropertyChanged(OrientationArgs);
                    break;
                }

                case nameof(PreferredTileSize):
                {
                    Size2 value = Converters.Convert<Size2>("Size2", entry.Value);
                    ValidateTileSize(value);
                    if (_preferredTileSize.Apply(value))
                        NotifyPropertyChanged(PreferredTileSizeArgs);
                    break;
                }

                case nameof(PreserveAspectRatio):
                {
                    bool value = Converters.Convert<bool>("Boolean", entry.Value);
                    if (_preserveAspectRatio.Apply(value))
                        NotifyPropertyChanged(PreserveAspectRatioArgs);
                    break;
                }

                case nameof(Spacing):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateSpacing(value);
                    if (_spacing.Apply(value))
                        NotifyPropertyChanged(SpacingArgs);
                    break;
                }
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property) => property switch
    {
        nameof(MinTileSize) => _minTileSize.HasLocalValue,
        nameof(Orientation) => _orientation.HasLocalValue,
        nameof(PreferredTileSize) => _preferredTileSize.HasLocalValue,
        nameof(PreserveAspectRatio) => _preserveAspectRatio.HasLocalValue,
        nameof(Spacing) => _spacing.HasLocalValue,
        _ => base.IsStyleLocal(property)};

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(MinTileSize):
                if (_minTileSize.ClearLocal())
                    NotifyPropertyChanged(MinTileSizeArgs);
                break;
            case nameof(Orientation):
                if (_orientation.ClearLocal())
                    NotifyPropertyChanged(OrientationArgs);
                break;
            case nameof(PreferredTileSize):
                if (_preferredTileSize.ClearLocal())
                    NotifyPropertyChanged(PreferredTileSizeArgs);
                break;
            case nameof(PreserveAspectRatio):
                if (_preserveAspectRatio.ClearLocal())
                    NotifyPropertyChanged(PreserveAspectRatioArgs);
                break;
            case nameof(Spacing):
                if (_spacing.ClearLocal())
                    NotifyPropertyChanged(SpacingArgs);
                break;
            default:
                base.ResetStyleProperty(property);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _minTileSize.BeginStyle(new Size2(16));
        _orientation.BeginStyle(StackOrientation.Horizontal);
        _preferredTileSize.BeginStyle(new Size2(128));
        _preserveAspectRatio.BeginStyle(true);
        _spacing.BeginStyle(8);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_minTileSize.CommitStyle())
            NotifyPropertyChanged(MinTileSizeArgs);
        if (_orientation.CommitStyle())
            NotifyPropertyChanged(OrientationArgs);
        if (_preferredTileSize.CommitStyle())
            NotifyPropertyChanged(PreferredTileSizeArgs);
        if (_preserveAspectRatio.CommitStyle())
            NotifyPropertyChanged(PreserveAspectRatioArgs);
        if (_spacing.CommitStyle())
            NotifyPropertyChanged(SpacingArgs);
    }
}
