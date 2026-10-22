using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Presents a control directly or displays an object's text representation.</summary>
public class ContentControl : ContainerControl
{
    private ChangeTracker<Control?> _content;
    /// <summary>Creates a content host.</summary>
    public ContentControl() : base()
    {
    }

    private object? _storedContent;
    private Control? _contentPresenter;
    private TextBlock? _textPresenter;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ContentStorageArgs = new(nameof(Content));
    private static readonly System.ComponentModel.PropertyChangedEventArgs ContentPresenterChangedArgs = new(nameof(ContentPresenter));

    /// <summary>Gets or sets the source content. Controls are presented directly; other values are converted to text when assigned.</summary>
    /// <remarks>Reassign an object to refresh its text representation. Null removes the presenter.</remarks>
    public object? Content
    {
        get => _storedContent;
        set
        {
            VerifyTreeMutation();
            // Convert before changing state: a failing ToString must preserve the existing content.
            string? text = value is null or Control ? null : value as string ?? value.ToString() ?? string.Empty;
            TextBlock? textPresenter = text is null ? null : _textPresenter ?? new TextBlock { Text = text };
            Control? next = value as Control ?? textPresenter;
            if (!ReferenceEquals(next, _contentPresenter) && next is not null)
                ValidateChild(next);
            bool changed = !ReferenceEquals(_storedContent, value);
            _storedContent = value;
            _textPresenter = textPresenter;
            SetOwnedControl(ref _contentPresenter, next, ContentPresenterChangedArgs);
            if (textPresenter is not null)
                textPresenter.Text = text!;
            if (changed)
                NotifyPropertyChanged(ContentStorageArgs);
        }
    }

    /// <summary>Gets the actual child presenting the content, or null when content is null.</summary>
    public Control? ContentPresenter => _contentPresenter;
    /// <summary>Gets or sets horizontal content alignment.</summary>
    public ControlAlignment ContentHorizontalAlignment { get => ConfigContentHorizontalAlignment; set => ConfigContentHorizontalAlignment = value; }
    /// <summary>Gets or sets vertical content alignment.</summary>
    public ControlAlignment ContentVerticalAlignment { get => ConfigContentVerticalAlignment; set => ConfigContentVerticalAlignment = value; }

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        ValidateChild(child);
        Content = child;
    }

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (ReferenceEquals(ContentPresenter, child))
            Content = null;
    }

    /// <inheritdoc/>
    public override void Clear() => Content = null;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "ContentControl";

    internal override void Prepare()
    {
        base.Prepare();
        Control? next = ContentPresenter;
        if (_content.WasEnsured && ReferenceEquals(_content.NewValue, next))
            return;
        if (next is not null)
            PresentOwnedChild(next, true);
        _content.EnsureChange(next);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (ContentPresenter is not Control child)
            return Size2.Zero;
        child.Measure(context, available);
        return child.DesiredSize;
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        if (ContentPresenter is not Control child)
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
