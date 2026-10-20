namespace Sachssoft.Engine.Controls;
/// <summary>A fixed-range or automatically scaled line, step or bar chart for live measurements.</summary>
public class Chart : Control
{
    /// <summary>Creates a chart with an observable series collection.</summary>
    public Chart()
    {
        Series = new(this, SeriesArgs);
    }

    private static readonly System.ComponentModel.PropertyChangedEventArgs SeriesArgs = new(nameof(Series));
    /// <summary>Gets the chart series; each has a bounded allocation-free sample buffer.</summary>
    public ElementCollection<ChartSeries> Series { get; }

    /// <summary>Gets or sets the geometry mode.</summary>
    public ChartKind Kind
    {
        get => _Kind.Value;
        set
        {
            if (_Kind.SetLocal(value))
                NotifyPropertyChanged(KindArgs);
        }
    }

    private StyleProperty<ChartKind> _Kind = new(ChartKind.Line);
    private static readonly System.ComponentModel.PropertyChangedEventArgs KindArgs = new(nameof(Kind));
    private readonly List<Vector2[]> _points = new();
    private readonly List<int> _counts = new();
    private bool _dirty = true;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Chart";

    /// <inheritdoc/>
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args)
    {
        _dirty = true;
        if (args.PropertyName != nameof(Series))
            base.OnPropertyChanged(args);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available) => new(260, 140);
    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        _dirty = true;
        Rebuild();
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        Rebuild();
    }

    private float _low, _high;
    private void Rebuild()
    {
        if (!_dirty)
            return;
        if (!EnumValid() || !float.IsFinite(Minimum) || !float.IsFinite(Maximum) || Maximum <= Minimum || !float.IsFinite(LineThickness) || LineThickness <= 0)
            throw new InvalidOperationException("Invalid chart range or geometry.");
        _low = Minimum;
        _high = Maximum;
        if (AutoScale)
        {
            _low = 0;
            _high = 0;
            for (int seriesIndex = 0; seriesIndex < Series.Count; seriesIndex++)
            {
                ChartSeries s = Series[seriesIndex];
                for (int i = 0; i < s.Count; i++)
                {
                    _low = MathF.Min(_low, s[i]);
                    _high = MathF.Max(_high, s[i]);
                }
            }

            if (_high <= _low)
                _high = _low + 1;
        }

        while (_points.Count < Series.Count)
        {
            _points.Add(Array.Empty<Vector2>());
            _counts.Add(0);
        }

        while (_points.Count > Series.Count)
        {
            int i = _points.Count - 1;
            _points.RemoveAt(i);
            _counts.RemoveAt(i);
        }

        float w = MathF.Max(0, Bounds.Width - Padding.Left - Padding.Right), h = MathF.Max(0, Bounds.Height - Padding.Top - Padding.Bottom);
        for (int s = 0; s < Series.Count; s++)
        {
            var series = Series[s];
            if (_points[s].Length < series.Capacity)
                _points[s] = new Vector2[series.Capacity];
            _counts[s] = series.Count;
            for (int i = 0; i < series.Count; i++)
                _points[s][i] = new(Padding.Left + (i + .5f) * w / Math.Max(1, series.Count), Padding.Top + h - (float)Math.Clamp(((double)series[i] - _low) / ((double)_high - _low), -1000000d, 1000000d) * h);
        }

        _dirty = false;
    }

    private bool EnumValid() => Kind is ChartKind.Line or ChartKind.Bar or ChartKind.Step;
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        bool skinned = context.DrawSkinPart(this, "Background", LocalBounds);
        if (!skinned)
            context.FillRectangle(LocalBounds, Background);
        float w = MathF.Max(0, Bounds.Width - Padding.Left - Padding.Right), h = MathF.Max(0, Bounds.Height - Padding.Top - Padding.Bottom);
        using (context.PushClipScope(new(Padding.Left, Padding.Top, w, h)))
        {
            for (int j = 0; j <= 4; j++)
            {
                float y = Padding.Top + j * h / 4;
                context.DrawLine(new(Padding.Left, y), new(Padding.Left + w, y), BorderColor);
            }

            float zero = Padding.Top + h - (float)((Math.Clamp(0d, _low, _high) - _low) / ((double)_high - _low)) * h;
            for (int s = 0; s < _points.Count; s++)
            {
                Color color = Series[s].Color;
                int count = _counts[s];
                for (int i = 0; i < count; i++)
                {
                    Vector2 p = _points[s][i];
                    if (Kind == ChartKind.Bar)
                    {
                        float bar = w / Math.Max(1, count) * .8f;
                        context.FillRectangle(new(p.X - bar / 2, MathF.Min(zero, p.Y), bar, MathF.Max(1, MathF.Abs(zero - p.Y))), color);
                    }
                    else if (i > 0)
                    {
                        Vector2 prev = _points[s][i - 1];
                        if (Kind == ChartKind.Step)
                        {
                            context.DrawLine(prev, new(p.X, prev.Y), color, LineThickness);
                            context.DrawLine(new(p.X, prev.Y), p, color, LineThickness);
                        }
                        else
                            context.DrawLine(prev, p, color, LineThickness);
                    }
                }
            }
        }

        if (!skinned)
            context.StrokeRectangle(LocalBounds, BorderColor, MathF.Max(1, BorderThickness));
    }

    private StyleProperty<bool> _AutoScale = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs AutoScaleArgs = new(nameof(AutoScale));
    /// <summary>Gets the effective AutoScale value or assigns a local override.</summary>
    public bool AutoScale
    {
        get => _AutoScale.Value;
        set
        {
            if (_AutoScale.SetLocal(value))
                NotifyPropertyChanged(AutoScaleArgs);
        }
    }

    private StyleProperty<float> _Minimum = new(0);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MinimumArgs = new(nameof(Minimum));
    /// <summary>Gets the effective Minimum value or assigns a local override.</summary>
    public float Minimum
    {
        get => _Minimum.Value;
        set
        {
            if (_Minimum.SetLocal(value))
                NotifyPropertyChanged(MinimumArgs);
        }
    }

    private StyleProperty<float> _Maximum = new(100);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MaximumArgs = new(nameof(Maximum));
    /// <summary>Gets the effective Maximum value or assigns a local override.</summary>
    public float Maximum
    {
        get => _Maximum.Value;
        set
        {
            if (_Maximum.SetLocal(value))
                NotifyPropertyChanged(MaximumArgs);
        }
    }

    private StyleProperty<float> _LineThickness = new(2);
    private static readonly System.ComponentModel.PropertyChangedEventArgs LineThicknessArgs = new(nameof(LineThickness));
    /// <summary>Gets the effective LineThickness value or assigns a local override.</summary>
    public float LineThickness
    {
        get => _LineThickness.Value;
        set
        {
            if (_LineThickness.SetLocal(value))
                NotifyPropertyChanged(LineThicknessArgs);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
            switch (entry.Key)
            {
                case "Kind":
                    if (_Kind.Apply(Converters.Convert<ChartKind>("ChartKind", entry.Value)))
                        NotifyPropertyChanged(KindArgs);
                    break;
                case "AutoScale":
                    if (_AutoScale.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(AutoScaleArgs);
                    break;
                case "Minimum":
                    if (_Minimum.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MinimumArgs);
                    break;
                case "Maximum":
                    if (_Maximum.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MaximumArgs);
                    break;
                case "LineThickness":
                    if (_LineThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(LineThicknessArgs);
                    break;
            }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        "Kind" => _Kind.HasLocalValue,
        "AutoScale" => _AutoScale.HasLocalValue,
        "Minimum" => _Minimum.HasLocalValue,
        "Maximum" => _Maximum.HasLocalValue,
        "LineThickness" => _LineThickness.HasLocalValue,
        _ => base.IsStyleLocal(property)};
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Kind":
                if (_Kind.ClearLocal())
                    NotifyPropertyChanged(KindArgs);
                break;
            case "AutoScale":
                if (_AutoScale.ClearLocal())
                    NotifyPropertyChanged(AutoScaleArgs);
                break;
            case "Minimum":
                if (_Minimum.ClearLocal())
                    NotifyPropertyChanged(MinimumArgs);
                break;
            case "Maximum":
                if (_Maximum.ClearLocal())
                    NotifyPropertyChanged(MaximumArgs);
                break;
            case "LineThickness":
                if (_LineThickness.ClearLocal())
                    NotifyPropertyChanged(LineThicknessArgs);
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
        _Kind.BeginStyle(ChartKind.Line);
        _AutoScale.BeginStyle(true);
        _Minimum.BeginStyle(0);
        _Maximum.BeginStyle(100);
        _LineThickness.BeginStyle(2);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_Kind.CommitStyle())
            NotifyPropertyChanged(KindArgs);
        if (_AutoScale.CommitStyle())
            NotifyPropertyChanged(AutoScaleArgs);
        if (_Minimum.CommitStyle())
            NotifyPropertyChanged(MinimumArgs);
        if (_Maximum.CommitStyle())
            NotifyPropertyChanged(MaximumArgs);
        if (_LineThickness.CommitStyle())
            NotifyPropertyChanged(LineThicknessArgs);
    }
}
