using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Hosts arbitrary content in an automatic, fixed or proportional status bar slot.</summary>
public class StatusStrip : ContentControl
{
    /// <summary>Caches the effective length notification.</summary>
    private static readonly PropertyChangedEventArgs LengthChangedArgs = new(nameof(Length));

    /// <summary>Stores the central style value and optional local length override.</summary>
    private StyleProperty<GridLength> _length = new(GridLength.Auto);

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(StatusStrip);

    /// <summary>
    /// Gets or sets the slot length along the containing StatusBar's orientation.
    /// Defaults to Auto. Proportional lengths divide remaining space by weight;
    /// under an unbounded measurement they use content size. Changes invalidate layout.
    /// </summary>
    public GridLength Length
    {
        get => _length.Value;
        set
        {
            if (_length.SetLocal(value))
                NotifyPropertyChanged(LengthChangedArgs);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        if (style.Values.TryGetValue(nameof(Length), out string? text) && _length.Apply(Converters.Convert<GridLength>("GridLength", text)))
            NotifyPropertyChanged(LengthChangedArgs);
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property) => property == nameof(Length)
        ? _length.HasLocalValue : base.IsStyleLocal(property);

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        if (property != nameof(Length))
            base.ResetStyleProperty(property);
        else if (_length.ClearLocal())
            NotifyPropertyChanged(LengthChangedArgs);
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _length.BeginStyle(GridLength.Auto);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_length.CommitStyle())
            NotifyPropertyChanged(LengthChangedArgs);
    }
}
