using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>Provides shared header and body presentation, sizing and ownership for content groups.</summary>
/// <remarks>HeaderContent is the only assignable header value. Its private host is retained for the group's lifetime;
/// HeaderContentPresenter exposes the actual presented control without exposing a replaceable host slot.</remarks>
public class HeaderContentControl : ContentControl
{
    private readonly ContentControl _headerHost;
    private Bounds2 _body;
    private static readonly PropertyChangedEventArgs HeaderContentArgs = new(nameof(HeaderContent));
    private static readonly PropertyChangedEventArgs HeaderPresenterArgs = new(nameof(HeaderContentPresenter));
    /// <summary>Creates an empty group with a private reusable header host.</summary>
    public HeaderContentControl() : this(new ContentControl { ContentHorizontalAlignment = ControlAlignment.Start })
    {
    }

    /// <summary>Creates a group with a persistent specialized header host.</summary>
    /// <param name = "headerHost">The unowned host for header content. Ownership is reserved immediately.</param>
    /// <exception cref = "ArgumentNullException">The header host is null.</exception>
    /// <exception cref = "InvalidOperationException">The host already has an owner or would introduce a cycle.</exception>
    protected HeaderContentControl(ContentControl headerHost)
    {
        ArgumentNullException.ThrowIfNull(headerHost);
        ReserveChild(headerHost);
        _headerHost = headerHost;
        _headerHost.PropertyChanged += OnHeaderPropertyChanged;
    }

    /// <summary>Gets or sets header data. Controls are presented directly; other values use their text representation.</summary>
    /// <remarks>Null removes the presented header content. Reassign an object to refresh its text representation.</remarks>
    public object? HeaderContent
    {
        get => _headerHost.Content;
        set
        {
            VerifyTreeMutation();
            _headerHost.Content = value;
        }
    }

    /// <summary>Gets the control presenting the header data, or null for empty header data.</summary>
    public Control? HeaderContentPresenter => _headerHost.ContentPresenter;

    private void OnHeaderPropertyChanged(object? sender, PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName == nameof(Content))
        {
            // Caption ownership is immediate; changes to the traversed host commit at a safe boundary.
            if (Workspace is Workspace workspace)
                workspace.Mutate(() => PresentOwnedChild(_headerHost, HeaderContent is not null));
            else
                PresentOwnedChild(_headerHost, HeaderContent is not null);
            NotifyPropertyChanged(HeaderContentArgs);
        }
        else if (notification.PropertyName == nameof(ContentPresenter))
            NotifyPropertyChanged(HeaderPresenterArgs);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "HeaderContentControl";
    /// <summary>Gets the larger of HeaderHeight and the measured header height.</summary>
    protected float EffectiveHeaderHeight => MathF.Max(HeaderHeight, HeaderContent is null ? 0 : _headerHost.DesiredSize.Height);

    internal override void Prepare()
    {
        base.Prepare();
        PresentOwnedChild(_headerHost, HeaderContent is not null);
    }

    internal override Bounds2 GetChildClip(Control child) => ReferenceEquals(child, _headerHost) ? ContentBounds : _body;
    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (ReferenceEquals(_headerHost, child) || ReferenceEquals(HeaderContentPresenter, child))
            HeaderContent = null;
        else
            base.Remove(child);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        base.Clear();
        HeaderContent = null;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!float.IsFinite(HeaderHeight) || HeaderHeight < 0)
            throw new InvalidOperationException("HeaderHeight must be finite and nonnegative.");
        if (HeaderContent is not null)
            _headerHost.Measure(context, available);
        Size2 content = base.MeasureOverride(context, new Size2(available.Width, MathF.Max(0, available.Height - EffectiveHeaderHeight)));
        return new Size2(MathF.Max(HeaderContent is null ? 0 : _headerHost.DesiredSize.Width, content.Width), EffectiveHeaderHeight + content.Height);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float height = MathF.Min(bounds.Height, EffectiveHeaderHeight);
        if (HeaderContent is not null)
            _headerHost.Arrange(new Bounds2(bounds.X, bounds.Y, bounds.Width, height), true);
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
