namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Grid row configuration.</summary>
public sealed class GridRow : GridDimension
{
    /// <summary>Creates a row with a proportional weight of one.</summary>
    public GridRow()
    {
    }

    /// <summary>Creates a row with the specified height.</summary>
    /// <param name="height">The fixed, automatic or proportional row height.</param>
    public GridRow(GridLength height)
    {
        Height = height;
    }

    /// <summary>Creates a row with an explicit sizing mode.</summary>
    /// <param name="value">The logical pixel height or proportional weight.</param>
    /// <param name="unit">The sizing mode.</param>
    public GridRow(float value, GridUnitType unit)
        : this(new GridLength(value, unit))
    {
    }

    /// <summary>Gets or sets the row length.</summary>
    public GridLength Height { get => Length; set => Length = value; }
    /// <summary>Gets or sets minimum row height.</summary>
    public float MinHeight { get => Minimum; set => Minimum = value; }
    /// <summary>Gets or sets maximum row height.</summary>
    public float MaxHeight { get => Maximum; set => Maximum = value; }
}
