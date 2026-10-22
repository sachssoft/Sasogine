using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>A clipped content viewport with two independent, reusable ScrollBar controls.</summary>
public sealed class ScrollViewer : ContainerControl
{
    private Control? _content;
    private ControlLayoutContext? _layout;
    private bool _horizontal, _vertical;
    private Size2 _extent;
    private Bounds2 _viewport;
    /// <summary>Initializes a new scroll viewer instance.</summary>
    public ScrollViewer() : base()
    {
        HorizontalScrollBar = new ScrollBar
        {
            Orientation = ControlOrientation.Horizontal
        };
        VerticalScrollBar = new ScrollBar
        {
            Orientation = ControlOrientation.Vertical
        };
        base.Add(HorizontalScrollBar);
        base.Add(VerticalScrollBar);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ScrollViewer";
    /// <summary>Gets horizontal scroll bar.</summary>
    public ScrollBar HorizontalScrollBar { get; }
    /// <summary>Gets vertical scroll bar.</summary>
    public ScrollBar VerticalScrollBar { get; }
    /// <summary>Gets viewport bounds.</summary>
    public Bounds2 ViewportBounds => _viewport;
    /// <summary>Gets extent.</summary>
    public Size2 Extent => _extent;
    /// <summary>Gets or sets horizontal scroll bar visibility.</summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility { get => ConfigHorizontalScrollBarVisibility; set => ConfigHorizontalScrollBarVisibility = value; }
    /// <summary>Gets or sets vertical scroll bar visibility.</summary>
    public ScrollBarVisibility VerticalScrollBarVisibility { get => ConfigVerticalScrollBarVisibility; set => ConfigVerticalScrollBarVisibility = value; }
    /// <summary>Gets or sets content.</summary>
    public Control? Content { get => ConfigContent; set => ConfigContent = value; }

    internal override void Prepare()
    {
        base.Prepare();
        if (ReferenceEquals(_content, ConfigContent))
            return;
        if (ConfigContent is Control child)
            PresentOwnedChild(child, true);
        _content = ConfigContent;
    }

    internal override Bounds2 GetChildClip(Control child) => child == _content ? _viewport : base.ContentBounds;
    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _layout = context;
        float thickness = ConfigScrollBarThickness;
        if (!float.IsFinite(thickness) || thickness <= 0)
            throw new InvalidOperationException("Invalid scrollbar thickness.");
        _content?.Measure(context, new Size2(HorizontalScrollBarVisibility == ScrollBarVisibility.Hidden ? available.Width : float.PositiveInfinity, VerticalScrollBarVisibility == ScrollBarVisibility.Hidden ? available.Height : float.PositiveInfinity));
        _extent = _content?.DesiredSize ?? Size2.Zero;
        _horizontal = HorizontalScrollBarVisibility == ScrollBarVisibility.Always;
        _vertical = VerticalScrollBarVisibility == ScrollBarVisibility.Always;
        for (int i = 0; i < 3; i++)
        {
            if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
                _horizontal = _extent.Width > MathF.Max(0, available.Width - (_vertical ? thickness : 0));
            if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
                _vertical = _extent.Height > MathF.Max(0, available.Height - (_horizontal ? thickness : 0));
        }

        // Measurement can be unbounded; publish visibility only for the arranged viewport.
        HorizontalScrollBar.Measure(context, available);
        VerticalScrollBar.Measure(context, available);
        return new Size2(MathF.Min(available.Width, _extent.Width + (_vertical ? thickness : 0)), MathF.Min(available.Height, _extent.Height + (_horizontal ? thickness : 0)));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        float t = ConfigScrollBarThickness;
        _horizontal = HorizontalScrollBarVisibility == ScrollBarVisibility.Always;
        _vertical = VerticalScrollBarVisibility == ScrollBarVisibility.Always;
        for (int i = 0; i < 3; i++)
        {
            if (_layout is not null && _content is not null)
            {
                _content.Measure(_layout, new Size2(HorizontalScrollBarVisibility == ScrollBarVisibility.Hidden ? MathF.Max(0, contentBounds.Width - (_vertical ? t : 0)) : float.PositiveInfinity, VerticalScrollBarVisibility == ScrollBarVisibility.Hidden ? MathF.Max(0, contentBounds.Height - (_horizontal ? t : 0)) : float.PositiveInfinity));
                _extent = _content.DesiredSize;
            }

            if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
                _horizontal = _extent.Width > MathF.Max(0, contentBounds.Width - (_vertical ? t : 0));
            if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
                _vertical = _extent.Height > MathF.Max(0, contentBounds.Height - (_horizontal ? t : 0));
        }

        HorizontalScrollBar.IsVisible = _horizontal;
        VerticalScrollBar.IsVisible = _vertical;
        if (_layout is not null)
        {
            HorizontalScrollBar.Measure(_layout, new Size2(contentBounds.Width, contentBounds.Height));
            VerticalScrollBar.Measure(_layout, new Size2(contentBounds.Width, contentBounds.Height));
        }

        _viewport = new Bounds2(contentBounds.X, contentBounds.Y, MathF.Max(0, contentBounds.Width - (_vertical ? t : 0)), MathF.Max(0, contentBounds.Height - (_horizontal ? t : 0)));
        HorizontalScrollBar.Maximum = MathF.Max(0, _extent.Width - _viewport.Width);
        HorizontalScrollBar.ViewportSize = _viewport.Width;
        HorizontalScrollBar.LargeChange = _viewport.Width;
        HorizontalScrollBar.SmallChange = 24;
        VerticalScrollBar.Maximum = MathF.Max(0, _extent.Height - _viewport.Height);
        VerticalScrollBar.ViewportSize = _viewport.Height;
        VerticalScrollBar.LargeChange = _viewport.Height;
        VerticalScrollBar.SmallChange = 24;
        HorizontalScrollBar.Arrange(new Bounds2(contentBounds.X, _viewport.Bottom, _viewport.Width, t), true);
        VerticalScrollBar.Arrange(new Bounds2(_viewport.Right, contentBounds.Y, t, _viewport.Height), true);
        _content?.Arrange(new Bounds2(_viewport.X - (_horizontal ? HorizontalScrollBar.Value : 0), _viewport.Y - (_vertical ? VerticalScrollBar.Value : 0), MathF.Max(_viewport.Width, _extent.Width), MathF.Max(_viewport.Height, _extent.Height)));
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type != ControlInputEventType.PointerWheel)
            return;
        ScrollBar bar = e.Shift && _horizontal ? HorizontalScrollBar : VerticalScrollBar;
        if (!bar.IsVisible)
            return;
        float old = bar.Value;
        bar.Value -= e.WheelDelta / 120f * bar.SmallChange;
        if (old != bar.Value)
            args.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (_horizontal && _vertical)
            context.DrawSkinPart(this, "Corner", new Bounds2(_viewport.Right - Bounds.X, _viewport.Bottom - Bounds.Y, ConfigScrollBarThickness, ConfigScrollBarThickness));
    }

    private StyleProperty<float> _styleScrollBarThickness = new(20f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ScrollBarThicknessChangedArgs = new(nameof(ScrollBarThickness));
    internal float ConfigScrollBarThickness
    {
        get => _styleScrollBarThickness.Value;
        set
        {
            if (_styleScrollBarThickness.SetLocal(value))
                NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        }
    }

    /// <summary>Gets or sets ScrollBarThickness; a local assignment overrides the style.</summary>
    public float ScrollBarThickness { get => ConfigScrollBarThickness; set => ConfigScrollBarThickness = value; }

    private StyleProperty<ScrollBarVisibility> _styleHorizontalScrollBarVisibility = new(ScrollBarVisibility.Auto);
    private static readonly System.ComponentModel.PropertyChangedEventArgs HorizontalScrollBarVisibilityChangedArgs = new(nameof(HorizontalScrollBarVisibility));
    internal ScrollBarVisibility ConfigHorizontalScrollBarVisibility
    {
        get => _styleHorizontalScrollBarVisibility.Value;
        set
        {
            if (_styleHorizontalScrollBarVisibility.SetLocal(value))
                NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
        }
    }

    private StyleProperty<ScrollBarVisibility> _styleVerticalScrollBarVisibility = new(ScrollBarVisibility.Auto);
    private static readonly System.ComponentModel.PropertyChangedEventArgs VerticalScrollBarVisibilityChangedArgs = new(nameof(VerticalScrollBarVisibility));
    internal ScrollBarVisibility ConfigVerticalScrollBarVisibility
    {
        get => _styleVerticalScrollBarVisibility.Value;
        set
        {
            if (_styleVerticalScrollBarVisibility.SetLocal(value))
                NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
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
                case "ScrollBarThickness":
                    if (_styleScrollBarThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                    break;
                case "HorizontalScrollBarVisibility":
                    if (_styleHorizontalScrollBarVisibility.Apply(Converters.Convert<ScrollBarVisibility>("ScrollBarVisibility", entry.Value)))
                        NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
                    break;
                case "VerticalScrollBarVisibility":
                    if (_styleVerticalScrollBarVisibility.Apply(Converters.Convert<ScrollBarVisibility>("ScrollBarVisibility", entry.Value)))
                        NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "ScrollBarThickness":
                return _styleScrollBarThickness.HasLocalValue;
            case "HorizontalScrollBarVisibility":
                return _styleHorizontalScrollBarVisibility.HasLocalValue;
            case "VerticalScrollBarVisibility":
                return _styleVerticalScrollBarVisibility.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "ScrollBarThickness":
                if (_styleScrollBarThickness.ClearLocal())
                    NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                return;
            case "HorizontalScrollBarVisibility":
                if (_styleHorizontalScrollBarVisibility.ClearLocal())
                    NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
                return;
            case "VerticalScrollBarVisibility":
                if (_styleVerticalScrollBarVisibility.ClearLocal())
                    NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
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
        if (_styleScrollBarThickness.BeginStyle(20f))
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleHorizontalScrollBarVisibility.BeginStyle(ScrollBarVisibility.Auto))
            NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
        if (_styleVerticalScrollBarVisibility.BeginStyle(ScrollBarVisibility.Auto))
            NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
    }

    private Control? _storedContent;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ContentChangedArgs = new(nameof(Content));
    internal Control? ConfigContent { get => _storedContent; set => SetOwnedControl(ref _storedContent, value, ContentChangedArgs); }

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        ValidateChild(child);
        Content = child;
    }

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (ReferenceEquals(Content, child))
            Content = null;
    }

    /// <inheritdoc/>
    public override void Clear() => Content = null;
    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleScrollBarThickness.CommitStyle())
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleHorizontalScrollBarVisibility.CommitStyle())
            NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
        if (_styleVerticalScrollBarVisibility.CommitStyle())
            NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
    }
}
