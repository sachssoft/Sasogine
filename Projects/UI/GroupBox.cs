namespace Sachssoft.Engine.Controls;
/// <summary>A bordered content group with an arbitrary control header.</summary>
public class GroupBox : ContainerControl
{
    private Control? _header, _content;
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderArgs = new(nameof(Header)), ContentArgs = new(nameof(Content));
    /// <summary>Gets or sets the header, including images or composed controls.</summary>
    public Control? Header { get => _header; set => SetOwnedControl(ref _header, value, HeaderArgs); }
    /// <summary>Gets or sets the group content.</summary>
    public Control? Content { get => _content; set => SetOwnedControl(ref _content, value, ContentArgs, ShowsContent); }
    /// <inheritdoc/>
    protected override string DefaultStyleName => "GroupBox";
    /// <summary>Gets whether the content participates in the control tree.</summary>
    protected virtual bool ShowsContent => true;
    /// <summary>Gets the header row height.</summary>
    protected float EffectiveHeaderHeight => MathF.Max(HeaderHeight, _header?.DesiredSize.Height ?? 0);

    internal override void Prepare()
    {
        base.Prepare();
        if (_header is not null)
            PresentOwnedChild(_header, true);
        if (_content is not null)
            PresentOwnedChild(_content, ShowsContent);
    }

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        ValidateChild(child);
        Content = child;
    }
    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        System.ArgumentNullException.ThrowIfNull(child);
        if (ReferenceEquals(Content, child))
            Content = null;
        else if (ReferenceEquals(Header, child))
            Header = null;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        Content = null;
        Header = null;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        Header?.Measure(context, available);
        if (ShowsContent)
            Content?.Measure(context, new Size2(available.Width, MathF.Max(0, available.Height - EffectiveHeaderHeight)));
        return new Size2(MathF.Max(Header?.DesiredSize.Width ?? 0, ShowsContent ? Content?.DesiredSize.Width ?? 0 : 0), EffectiveHeaderHeight + (ShowsContent ? Content?.DesiredSize.Height ?? 0 : 0));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float h = MathF.Min(b.Height, EffectiveHeaderHeight);
        Header?.Arrange(new(b.X, b.Y, b.Width, h), true);
        if (ShowsContent)
            Content?.Arrange(new(b.X, b.Y + h, b.Width, MathF.Max(0, b.Height - h)), true);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        if (!context.DrawSkinPart(this, "Background", LocalBounds))
        {
            context.FillRectangle(LocalBounds, Background);
            context.StrokeRectangle(LocalBounds, BorderColor, MathF.Max(1, BorderThickness));
        }
    }

    private StyleProperty<float> _HeaderHeight = new(30);
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderHeightArgs = new(nameof(HeaderHeight));
    /// <summary>Gets the effective HeaderHeight value or assigns a local override.</summary>
    public float HeaderHeight
    {
        get => _HeaderHeight.Value;
        set
        {
            if (_HeaderHeight.SetLocal(value))
                NotifyPropertyChanged(HeaderHeightArgs);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
            switch (entry.Key)
            {
                case "HeaderHeight":
                    if (_HeaderHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(HeaderHeightArgs);
                    break;
            }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        "HeaderHeight" => _HeaderHeight.HasLocalValue,
        _ => base.IsStyleLocal(property)};
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "HeaderHeight":
                if (_HeaderHeight.ClearLocal())
                    NotifyPropertyChanged(HeaderHeightArgs);
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
        _HeaderHeight.BeginStyle(30);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_HeaderHeight.CommitStyle())
            NotifyPropertyChanged(HeaderHeightArgs);
    }
}
