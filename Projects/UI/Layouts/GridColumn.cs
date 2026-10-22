namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Grid column configuration.</summary>
public sealed class GridColumn : GridDimension
{
    /// <summary>Creates a column with a proportional weight of one.</summary>
    public GridColumn()
    {
    }

    /// <summary>Creates a column with the specified width.</summary>
    /// <param name="width">The fixed, automatic or proportional column width.</param>
    public GridColumn(GridLength width)
    {
        Width = width;
    }

    /// <summary>Creates a column with an explicit sizing mode.</summary>
    /// <param name="value">The logical pixel width or proportional weight.</param>
    /// <param name="unit">The sizing mode.</param>
    public GridColumn(float value, GridUnitType unit)
        : this(new GridLength(value, unit))
    {
    }

    /// <summary>Gets or sets the column length.</summary>
    public GridLength Width { get => Length; set => Length = value; }
    /// <summary>Gets or sets minimum column width.</summary>
    public float MinWidth { get => Minimum; set => Minimum = value; }
    /// <summary>Gets or sets maximum column width.</summary>
    public float MaxWidth { get => Maximum; set => Maximum = value; }
}
