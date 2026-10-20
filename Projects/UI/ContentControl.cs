using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Measures and arranges one child; content can be an image, label or composed layout.</summary>
public class ContentControl : ContainerControl
{
    private ChangeTracker<Control?> _content;
    /// <summary>Creates a content host.</summary>
    public ContentControl() : base()
    {
    }

    /// <summary>Gets or sets arbitrary content. Changes commit at a layout boundary.</summary>
    public Control? Content { get => ConfigContent; set => ConfigContent = value; }
    /// <summary>Gets or sets horizontal content alignment.</summary>
    public ControlAlignment ContentHorizontalAlignment { get => ConfigContentHorizontalAlignment; set => ConfigContentHorizontalAlignment = value; }
    /// <summary>Gets or sets vertical content alignment.</summary>
    public ControlAlignment ContentVerticalAlignment { get => ConfigContentVerticalAlignment; set => ConfigContentVerticalAlignment = value; }

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
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
    /// <inheritdoc/>
    protected override string DefaultStyleName => "ContentControl";

    internal override void Prepare()
    {
        base.Prepare();
        Control? next = ConfigContent;
        if (_content.WasEnsured && ReferenceEquals(_content.NewValue, next))
            return;
        ReplaceOwnedChild(_content.WasEnsured ? _content.NewValue : null, next);
        _content.EnsureChange(next);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (Content is not Control child)
            return Size2.Zero;
        child.Measure(context, available);
        return child.DesiredSize;
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        if (Content is not Control child)
            return;
        float w = ContentHorizontalAlignment == ControlAlignment.Stretch ? bounds.Width : MathF.Min(bounds.Width, child.DesiredSize.Width);
        float h = ContentVerticalAlignment == ControlAlignment.Stretch ? bounds.Height : MathF.Min(bounds.Height, child.DesiredSize.Height);
        static float Offset(float space, ControlAlignment align) => align == ControlAlignment.Center ? space / 2 : align == ControlAlignment.End ? space : 0;
        child.Arrange(new Bounds2(bounds.X + Offset(bounds.Width - w, ContentHorizontalAlignment), bounds.Y + Offset(bounds.Height - h, ContentVerticalAlignment), w, h), true);
    }

    private StyleProperty<ControlAlignment> _styleContentHorizontalAlignment = new(ControlAlignment.Center);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ContentHorizontalAlignmentChangedArgs = new(nameof(ContentHorizontalAlignment));
    internal ControlAlignment ConfigContentHorizontalAlignment
    {
        get => _styleContentHorizontalAlignment.Value;
        set
        {
            if (_styleContentHorizontalAlignment.SetLocal(value))
                NotifyPropertyChanged(ContentHorizontalAlignmentChangedArgs);
        }
    }

    private StyleProperty<ControlAlignment> _styleContentVerticalAlignment = new(ControlAlignment.Center);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ContentVerticalAlignmentChangedArgs = new(nameof(ContentVerticalAlignment));
    internal ControlAlignment ConfigContentVerticalAlignment
    {
        get => _styleContentVerticalAlignment.Value;
        set
        {
            if (_styleContentVerticalAlignment.SetLocal(value))
                NotifyPropertyChanged(ContentVerticalAlignmentChangedArgs);
        }
    }

    private Control? _storedContent = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ContentStorageArgs = new("Content");
    internal Control? ConfigContent { get => _storedContent; set => SetNotify(ref _storedContent, value, ContentStorageArgs); }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "ContentHorizontalAlignment":
                    if (_styleContentHorizontalAlignment.Apply(Converters.Convert<ControlAlignment>("ControlAlignment", entry.Value)))
                        NotifyPropertyChanged(ContentHorizontalAlignmentChangedArgs);
                    break;
                case "ContentVerticalAlignment":
                    if (_styleContentVerticalAlignment.Apply(Converters.Convert<ControlAlignment>("ControlAlignment", entry.Value)))
                        NotifyPropertyChanged(ContentVerticalAlignmentChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "ContentHorizontalAlignment":
                return _styleContentHorizontalAlignment.HasLocalValue;
            case "ContentVerticalAlignment":
                return _styleContentVerticalAlignment.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "ContentHorizontalAlignment":
                if (_styleContentHorizontalAlignment.ClearLocal())
                    NotifyPropertyChanged(ContentHorizontalAlignmentChangedArgs);
                return;
            case "ContentVerticalAlignment":
                if (_styleContentVerticalAlignment.ClearLocal())
                    NotifyPropertyChanged(ContentVerticalAlignmentChangedArgs);
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
        if (_styleContentHorizontalAlignment.BeginStyle(ControlAlignment.Center))
            NotifyPropertyChanged(ContentHorizontalAlignmentChangedArgs);
        if (_styleContentVerticalAlignment.BeginStyle(ControlAlignment.Center))
            NotifyPropertyChanged(ContentVerticalAlignmentChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleContentHorizontalAlignment.CommitStyle())
            NotifyPropertyChanged(ContentHorizontalAlignmentChangedArgs);
        if (_styleContentVerticalAlignment.CommitStyle())
            NotifyPropertyChanged(ContentVerticalAlignmentChangedArgs);
    }
}
