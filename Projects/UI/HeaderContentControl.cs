using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>Provides shared header and body presentation, sizing and ownership for content groups.</summary>
/// <remarks>Content uses ContentControl's object-to-text conversion. HeaderContent uses a reusable content host;
/// Header remains available for assigning a header control directly.</remarks>
public class HeaderContentControl : ContentControl
{
    private Control? _header;
    private ContentControl? _headerHost;
    private Bounds2 _body;
    private static readonly PropertyChangedEventArgs HeaderArgs = new(nameof(Header));
    private static readonly PropertyChangedEventArgs HeaderContentArgs = new(nameof(HeaderContent));
    private static readonly PropertyChangedEventArgs HeaderPresenterArgs = new(nameof(HeaderContentPresenter));
    /// <summary>Creates an empty header and content group.</summary>
    public HeaderContentControl()
    {
    }

    /// <summary>Creates a group with a persistent specialized header host.</summary>
    /// <param name = "headerHost">The unowned host for header content.</param>
    /// <exception cref = "ArgumentNullException">The header host is null.</exception>
    protected HeaderContentControl(ContentControl headerHost)
    {
        ArgumentNullException.ThrowIfNull(headerHost);
        _headerHost = headerHost;
        _headerHost.PropertyChanged += OnHeaderPropertyChanged;
        Header = _headerHost;
    }

    /// <summary>Gets or sets the directly owned header control. Null removes the header.</summary>
    public Control? Header
    {
        get => _header;
        set
        {
            if (ReferenceEquals(_header, value))
                return;
            VerifyTreeMutation();
            if (value is not null)
                ValidateChild(value);
            object? oldContent = HeaderContent;
            Control? oldPresenter = HeaderContentPresenter;
            bool releaseHostContent = ReferenceEquals(_header, _headerHost);
            SetOwnedControl(ref _header, value, HeaderArgs);
            if (releaseHostContent && _headerHost is not null)
                _headerHost.Content = null;
            if (!ReferenceEquals(oldContent, HeaderContent))
                NotifyPropertyChanged(HeaderContentArgs);
            if (!ReferenceEquals(oldPresenter, HeaderContentPresenter))
                NotifyPropertyChanged(HeaderPresenterArgs);
        }
    }

    /// <summary>Gets or sets header data. Controls are presented directly; other values use their text representation.</summary>
    /// <remarks>Assigning this property restores the shared header host after a direct Header assignment.</remarks>
    public object? HeaderContent
    {
        get => ReferenceEquals(_header, _headerHost) ? _headerHost?.Content : _header;
        set
        {
            VerifyTreeMutation();
            if (value is Control control)
                for (Control? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
                    if (ReferenceEquals(control, ancestor))
                        throw new InvalidOperationException("Header content cannot contain its owner or an ancestor.");
            if (_headerHost is null)
            {
                _headerHost = new ContentControl
                {
                    ContentHorizontalAlignment = ControlAlignment.Start
                };
                _headerHost.PropertyChanged += OnHeaderPropertyChanged;
            }

            if (!ReferenceEquals(_header, _headerHost))
                ValidateChild(_headerHost);
            _headerHost.Content = value;
            Header = _headerHost;
        }
    }

    /// <summary>Gets the control presenting the header data, or null for empty header data.</summary>
    public Control? HeaderContentPresenter => ReferenceEquals(_header, _headerHost) ? _headerHost?.ContentPresenter : _header;

    private void OnHeaderPropertyChanged(object? sender, PropertyChangedEventArgs notification)
    {
        if (!ReferenceEquals(_header, _headerHost))
            return;
        if (notification.PropertyName == nameof(Content))
            NotifyPropertyChanged(HeaderContentArgs);
        else if (notification.PropertyName == nameof(ContentPresenter))
            NotifyPropertyChanged(HeaderPresenterArgs);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "HeaderContentControl";
    /// <summary>Gets the larger of HeaderHeight and the measured header height.</summary>
    protected float EffectiveHeaderHeight => MathF.Max(HeaderHeight, _header?.DesiredSize.Height ?? 0);

    internal override void Prepare()
    {
        base.Prepare();
        if (_header is not null)
            PresentOwnedChild(_header, true);
    }

    internal override Bounds2 GetChildClip(Control child) => ReferenceEquals(child, _header) ? ContentBounds : _body;
    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (ReferenceEquals(_header, child))
            Header = null;
        else
            base.Remove(child);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        base.Clear();
        Header = null;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!float.IsFinite(HeaderHeight) || HeaderHeight < 0)
            throw new InvalidOperationException("HeaderHeight must be finite and nonnegative.");
        _header?.Measure(context, available);
        Size2 content = base.MeasureOverride(context, new Size2(available.Width, MathF.Max(0, available.Height - EffectiveHeaderHeight)));
        return new Size2(MathF.Max(_header?.DesiredSize.Width ?? 0, content.Width), EffectiveHeaderHeight + content.Height);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float height = MathF.Min(bounds.Height, EffectiveHeaderHeight);
        _header?.Arrange(new Bounds2(bounds.X, bounds.Y, bounds.Width, height), true);
        _body = new Bounds2(bounds.X, bounds.Y + height, bounds.Width, MathF.Max(0, bounds.Height - height));
        base.ArrangeOverride(_body);
    }

    private StyleProperty<float> _HeaderHeight = new(30);
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderHeightArgs = new(nameof(HeaderHeight));
    /// <summary>
    /// Gets the effective HeaderHeight value or assigns a local override.
    /// </summary>
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
