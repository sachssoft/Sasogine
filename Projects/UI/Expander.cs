namespace Sachssoft.Engine.Controls;
/// <summary>
/// A collapsible content group with a keyboard-accessible toggle header.
/// </summary>
public class Expander : GroupBox
{
    private readonly ExpanderHeaderButton _toggle = new();
    /// <summary>
    /// Creates an expanded group. Assign HeaderContent to customize the toggle label.
    /// </summary>
    public Expander()
    {
        Header = _toggle;
        _toggle.Clicked += (_, _) => IsExpanded = _toggle.IsChecked == true;
    }

    /// <summary>
    /// Gets or sets the content inside the toggle header.
    /// </summary>
    public object? HeaderContent
    {
        get => _toggle.Content;
        set
        {
            _toggle.Content = value;
            NotifyPropertyChanged(HeaderContentArgs);
        }
    }

    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderContentArgs = new(nameof(HeaderContent));
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Expander";
    /// <inheritdoc/>
    protected override bool ShowsContent => IsExpanded;

    internal override void Prepare()
    {
        _toggle.IsChecked = IsExpanded;
        base.Prepare();
    }

    private StyleProperty<bool> _IsExpanded = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsExpandedArgs = new(nameof(IsExpanded));
    /// <summary>
    /// Gets the effective IsExpanded value or assigns a local override.
    /// </summary>
    public bool IsExpanded
    {
        get => _IsExpanded.Value;
        set
        {
            if (_IsExpanded.SetLocal(value))
                NotifyPropertyChanged(IsExpandedArgs);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
            switch (entry.Key)
            {
                case "IsExpanded":
                    if (_IsExpanded.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsExpandedArgs);
                    break;
            }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        "IsExpanded" => _IsExpanded.HasLocalValue,
        _ => base.IsStyleLocal(property)
    };
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "IsExpanded":
                if (_IsExpanded.ClearLocal())
                    NotifyPropertyChanged(IsExpandedArgs);
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
        _IsExpanded.BeginStyle(true);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_IsExpanded.CommitStyle())
            NotifyPropertyChanged(IsExpandedArgs);
    }
}